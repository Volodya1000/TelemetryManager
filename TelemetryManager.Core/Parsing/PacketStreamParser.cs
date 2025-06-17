using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Parsing;

public class PacketStreamParser
{
    private const int MaxPacketSize = 1024; // Максимальный размер пакета для предотвращения переполнения
    private readonly Stream _stream;
    private readonly byte[] _syncMarkerBytes;
    private readonly byte[] _readBuffer = new byte[MaxPacketSize];

    public PacketStreamParser(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _syncMarkerBytes = BitConverter.GetBytes(PacketConstants.SyncMarker);

        // Убедиться, что байты синхромаркера в правильном порядке (big-endian)
        if (BitConverter.IsLittleEndian)
            Array.Reverse(_syncMarkerBytes);
    }

    public List<TelemetryPacket> Parse()
    {
        var packets = new List<TelemetryPacket>();

        while (true)
        {
            long syncPosition = FindSyncMarker();
            if (syncPosition == -1)
                break;

            _stream.Position = syncPosition;

            Header header = ReadHeader();
            if (header == null)
            {
                Console.WriteLine($"Invalid header after sync marker at position {syncPosition}");
                continue;
            }

            byte[] content = ReadContent(header.Size);
            if (content == null)
            {
                Console.WriteLine($"Failed to read content for packet at position {syncPosition}");
                continue;
            }

            // Пропустить padding
            int paddingSize = (header.Size % 2 == 0) ? 0 : 1;
            if (_stream.ReadByte() == -1 && paddingSize > 0)
            {
                Console.WriteLine($"Failed to read padding for packet at position {syncPosition}");
                continue;
            }

            // Прочитать контрольную сумму
            byte[] csBytes = new byte[2];
            if (_stream.Read(csBytes, 0, 2) != 2)
            {
                Console.WriteLine($"Failed to read checksum for packet at position {syncPosition}");
                continue;
            }

            ushort expectedCs = BitConverter.ToUInt16(csBytes, 0);
            byte[] dataForChecksum = GetDataForChecksum(header, content, paddingSize);
            ushort actualCs = ComputeChecksum(dataForChecksum);

            if (expectedCs != actualCs)
            {
                Console.WriteLine($"Checksum mismatch for packet at position {syncPosition}. Expected: {expectedCs}, Actual: {actualCs}");
                continue;
            }

            ISensorData sensorData;
            try
            {
                sensorData = SensorDataFactory.CreateParser(header.TypeId);
                sensorData.Parse(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating or parsing sensor data: {ex.Message}");
                continue;
            }

            var packet = new TelemetryPacket(
                header.Time,
                header.DevId,
                header.TypeId,
                header.SourceId,
                content,
                sensorData
            );

            packets.Add(packet);
        }

        return packets;
    }

    private long FindSyncMarker()
    {
        byte[] buffer = new byte[4];
        int bytesRead;

        while ((bytesRead = _stream.Read(buffer, 0, 1)) > 0)
        {
            // Если текущий байт совпадает с первым байтом синхромаркера
            if (buffer[0] == _syncMarkerBytes[0])
            {
                // Сохранить позицию начала возможного синхромаркера
                long potentialPosition = _stream.Position - 1;

                // Прочитать оставшиеся 3 байта
                if (_stream.Read(buffer, 1, 3) == 3)
                {
                    if (CompareBytes(buffer, _syncMarkerBytes))
                    {
                        return potentialPosition;
                    }
                }
                else
                {
                    // Не удалось прочитать оставшиеся байты - вернуться к началу
                    _stream.Position = potentialPosition + 1;
                }
            }
        }

        return -1; // Синхромаркер не найден
    }

    private bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }

    private Header ReadHeader()
    {
        byte[] headerBuffer = new byte[12]; // 4 (Time) + 2 (DevId) + 1 (TypeId) + 1 (SourceId) + 2 (Size) = 10 байт

        if (_stream.Read(headerBuffer, 0, 10) != 10)
            return null;

        // Восстановить порядок байт для big-endian
        if (BitConverter.IsLittleEndian)
            Array.Reverse(headerBuffer, 0, 4); // Time

        uint time = BitConverter.ToUInt32(headerBuffer, 0);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(headerBuffer, 4, 2); // DevId

        ushort devId = BitConverter.ToUInt16(headerBuffer, 4);

        SensorType type;
        try
        {
            type = (SensorType)headerBuffer[6]; // TypeId
        }
        catch
        {
            return null; // Неверный тип датчика
        }

        byte sourceId = headerBuffer[7]; // SourceId

        if (BitConverter.IsLittleEndian)
            Array.Reverse(headerBuffer, 8, 2); // Size

        ushort size = BitConverter.ToUInt16(headerBuffer, 8);

        // Проверить размер данных
        if (size > MaxPacketSize)
            return null;

        return new Header(time, devId, type, sourceId, size);
    }

    private byte[] ReadContent(int size)
    {
        byte[] content = new byte[size];
        int totalRead = 0;

        while (totalRead < size)
        {
            int bytesRead = _stream.Read(content, totalRead, size - totalRead);
            if (bytesRead == 0)
                return null; // Достигнут конец потока

            totalRead += bytesRead;
        }

        return content;
    }

    private byte[] GetDataForChecksum(Header header, byte[] content, int paddingSize)
    {
        // Подготовить данные для контрольной суммы: 
        // заголовок без синхромаркера + content + padding

        int headerSize = 10; // Размер заголовка без синхромаркера
        byte[] headerBytes = new byte[headerSize];

        // Записать Time
        byte[] timeBytes = BitConverter.GetBytes(header.Time);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);
        Array.Copy(timeBytes, 0, headerBytes, 0, 4);

        // Записать DevId
        byte[] devIdBytes = BitConverter.GetBytes(header.DevId);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(devIdBytes);
        Array.Copy(devIdBytes, 0, headerBytes, 4, 2);

        // Записать TypeId и SourceId
        headerBytes[6] = (byte)header.TypeId;
        headerBytes[7] = header.SourceId;

        // Записать Size
        byte[] sizeBytes = BitConverter.GetBytes(header.Size);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sizeBytes);
        Array.Copy(sizeBytes, 0, headerBytes, 8, 2);

        // Подготовить padding
        byte[] padding = new byte[paddingSize];

        // Объединить все части
        byte[] result = new byte[headerSize + content.Length + padding.Length];
        Buffer.BlockCopy(headerBytes, 0, result, 0, headerSize);
        Buffer.BlockCopy(content, 0, result, headerSize, content.Length);
        Buffer.BlockCopy(padding, 0, result, headerSize + content.Length, padding.Length);

        return result;
    }

    private ushort ComputeChecksum(byte[] data)
    {
        uint checksum = 0;
        int length = data.Length;

        // Суммировать все 16-битные слова
        for (int i = 0; i < length - 1; i += 2)
        {
            // Объединить два байта в 16-битное слово
            ushort word = BitConverter.ToUInt16(data, i);
            checksum += word;

            // Выполнить перенос старших битов
            if (checksum > 0xFFFF)
                checksum = (checksum & 0xFFFF) + 1;
        }

        // Если длина нечетная, добавить последний байт
        if (length % 2 == 1)
        {
            byte lastByte = data[length - 1];
            // Явно привести к uint перед сложением
            checksum += ((uint)lastByte) << 8;

            if (checksum > 0xFFFF)
                checksum = (checksum & 0xFFFF) + 1;
        }

        // Вернуть дополнение до 1
        return (ushort)(~checksum & 0xFFFF);
    }

    // Вспомогательная структура для хранения заголовка
    private class Header
    {
        public uint Time { get; }
        public ushort DevId { get; }
        public SensorType TypeId { get; }
        public byte SourceId { get; }
        public ushort Size { get; }

        public Header(uint time, ushort devId, SensorType typeId, byte sourceId, ushort size)
        {
            Time = time;
            DevId = devId;
            TypeId = typeId;
            SourceId = sourceId;
            Size = size;
        }
    }
}

public static class PacketConstants
{
    public static uint SyncMarker = 0xFAA0055F; // Big-endian: FA A0 05 5F
}
