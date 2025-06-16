using TelemetryManager.Core.Data;

namespace TelemetryManager.Core.Parsing;

public class PacketStreamParser
{
    private readonly Stream _stream;
    private readonly List<byte> _buffer = new();
    private const int MaxBufferReserve = 3;
    private const int ReadBufferSize = 4096;
    private static readonly byte[] SyncMarker = { 0xFA, 0xA0, 0x05, 0x5F };

    public event Action<string> ErrorOccurred;

    public PacketStreamParser(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public TelemetryPacket ReadNextPacket()
    {
        while (true)
        {
            // Поиск синхромаркера в буфере
            int syncIndex = FindSyncMarker();

            // Обработка случая отсутствия маркера
            if (syncIndex < 0)
            {
                // Очистка буфера с сохранением потенциального начала маркера
                if (_buffer.Count > MaxBufferReserve)
                {
                    int removeCount = _buffer.Count - MaxBufferReserve;
                    var noise = _buffer.Take(removeCount).ToArray();
                    ErrorOccurred?.Invoke($"Discarded {removeCount} noise bytes: {BitConverter.ToString(noise)}");
                    _buffer.RemoveRange(0, removeCount);
                }

                // Попытка чтения дополнительных данных
                if (!ReadFromStream())
                    return null; // Данных в потоке больше нет

                continue;
            }

            // Удаление шумовых байтов перед маркером
            if (syncIndex > 0)
            {
                var noise = _buffer.Take(syncIndex).ToArray();
                ErrorOccurred?.Invoke($"Discarded {syncIndex} noise bytes: {BitConverter.ToString(noise)}");
                _buffer.RemoveRange(0, syncIndex);
            }

            // Проверка наличия полного заголовка
            if (_buffer.Count < TelemetryPacket.HeaderSize)
            {
                if (!ReadFromStream())
                    return null; // Недостаточно данных для заголовка
                continue;
            }

            // Извлечение размера контента из заголовка
            ushort contentSize = (ushort)((_buffer[10] << 8) | _buffer[11]);
            int padding = contentSize % 2 == 0 ? 0 : 1;
            int totalPacketSize = TelemetryPacket.HeaderSize + contentSize + padding + 2;

            // Проверка наличия полного пакета
            if (_buffer.Count < totalPacketSize)
            {
                if (!ReadFromStream())
                    return null; // Недостаточно данных для полного пакета
                continue;
            }

            // Извлечение и обработка данных пакета
            var packetData = _buffer.Take(totalPacketSize).ToArray();
            _buffer.RemoveRange(0, totalPacketSize);

            try
            {
                return TelemetryPacket.Parse(packetData);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Packet parsing failed: {ex.Message}");
                // Продолжаем обработку следующего пакета
            }
        }
    }

    private bool ReadFromStream()
    {
        try
        {
            byte[] tempBuffer = new byte[ReadBufferSize];
            int bytesRead = _stream.Read(tempBuffer, 0, tempBuffer.Length);

            if (bytesRead == 0)
                return false; // Конец потока

            _buffer.AddRange(tempBuffer.Take(bytesRead));
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"Stream read error: {ex.Message}");
            return false;
        }
    }

    private int FindSyncMarker()
    {
        for (int i = 0; i <= _buffer.Count - SyncMarker.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < SyncMarker.Length; j++)
            {
                if (_buffer[i + j] != SyncMarker[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }
}