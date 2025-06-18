using TelemetryManager.Core.Enums;

namespace TelemetryManager.Persistence.Entities;

public class TelemetryPacketEntity
{
    public int Id { get; set; }
    public DateTime Time { get; set; } 
    public ushort DevId { get; set; }
    public SensorType SensorType { get; set; }
    public byte SensorSourceId { get; set; }
    public List<ContentItemEntity> ContentItems { get; set; } = new();
}