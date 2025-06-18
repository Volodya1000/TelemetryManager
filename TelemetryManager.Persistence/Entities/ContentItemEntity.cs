namespace TelemetryManager.Persistence.Entities;

public class ContentItemEntity
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public double Value { get; set; }
    public int TelemetryPacketId { get; set; }
    public TelemetryPacketEntity Packet { get; set; } = null!;
}