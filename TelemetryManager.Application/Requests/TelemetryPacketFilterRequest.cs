namespace TelemetryManager.Application.Requests;

public class TelemetryPacketFilterRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ushort? DeviceId { get; set; }
    public byte? SensorType { get; set; }
    public byte? SensorId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}