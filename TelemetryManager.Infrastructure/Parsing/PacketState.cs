namespace TelemetryManager.Infrastructure.Parsing;

public class PacketState
{
    public long PacketStart { get; set; }
    public byte[] Header { get; set; }
    public uint Timestamp { get; set; }
    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public ushort Size { get; set; }
    public byte[] Content { get; set; }
    public int Padding { get; set; }
}
