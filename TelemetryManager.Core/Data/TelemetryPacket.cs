using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data;

public class TelemetryPacket
{
    public const int HeaderSize = 14; // Sync(4) + Time(4) + DevId(2) + TypeId(1) + SourceId(1) + Size(2)
    private const uint SyncMarker = 0xFA_A0_05_5F;

    public uint Time { get; private set; }
    public ushort DevId { get; private set; }
    public SensorType TypeId { get; private set; }
    public byte SourceId { get; private set; }
    public byte[] Content { get; private set; }
    public ISensorData ParsedContent { get; private set; }

    public static TelemetryPacket Parse(byte[] packetData)
    {
        using var reader = new BinaryReader(new MemoryStream(packetData));

        // Проверка синхромаркера (big-endian)
        if (reader.ReadUInt32() != SyncMarker)
            throw new InvalidDataException("Invalid sync marker");

        // Чтение заголовка (big-endian порядок)
        var packet = new TelemetryPacket
        {
            Time = reader.ReadUInt32(),
            DevId = reader.ReadUInt16(),
            TypeId = (SensorType)reader.ReadByte(),
            SourceId = reader.ReadByte(),
        };

        // Размер контента (big-endian)
        ushort contentSize = reader.ReadUInt16();

        // Чтение содержимого
        packet.Content = reader.ReadBytes(contentSize);

        // Пропуск padding (если размер нечетный)
        if ((contentSize % 2) != 0)
        {
            reader.ReadByte();
        }

        // Проверка контрольной суммы
        ushort receivedChecksum = reader.ReadUInt16();
        ushort calculatedChecksum = CalculateChecksum(packetData, 0, packetData.Length - 2);

        if (receivedChecksum != calculatedChecksum)
            throw new InvalidDataException($"Checksum mismatch: {receivedChecksum} != {calculatedChecksum}");

        // Парсинг содержимого
        packet.ParsedContent = SensorDataFactory.CreateParser(packet.TypeId);
        packet.ParsedContent.Parse(packet.Content);

        return packet;
    }

    private static ushort CalculateChecksum(byte[] data, int offset, int length)
    {
        uint sum = 0;

        // Обработка по 16-битным словам
        for (int i = offset; i < offset + length; i += 2)
        {
            ushort word = (ushort)((data[i] << 8) + (i + 1 < offset + length ? data[i + 1] : 0));
            sum += word;
        }

        // Свёртывание переносов
        while ((sum >> 16) != 0)
            sum = (sum & 0xFFFF) + (sum >> 16);

        return (ushort)~sum;
    }
}