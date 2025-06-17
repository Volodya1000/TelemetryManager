using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data;

public class TelemetryPacket
{
    public const int HeaderSize = 14; // Sync(4) + Time(4) + DevId(2) + TypeId(1) + SourceId(1) + Size(2)

    public uint Time { get; }
    public ushort DevId { get; }
    public SensorType TypeId { get; }
    public byte SourceId { get; }
    public byte[] Content { get; }
    public ISensorData ParsedContent { get; }

    public TelemetryPacket(
        uint time,
        ushort devId,
        SensorType typeId,
        byte sourceId,
        byte[] content,
        ISensorData parsedContent)
    {
        Time = time;
        DevId = devId;
        TypeId = typeId;
        SourceId = sourceId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        ParsedContent = parsedContent;
    }
}
