namespace TelemetryManager.Application.OutputDtos;

public class ContentTypeFilterDto
{
    public byte Id { get; set; }
    public string Name { get; set; }
}

public class SensorFilterDto
{
    public byte SensorTypeId { get; set; }
    public byte SensorSourceId { get; set; }
    public string Name { get; set; }
    public ContentTypeFilterDto ContentType { get; set; }
}

public class DeviceFilterDto
{
    public ushort DeviceId { get; set; }
    public string Name { get; set; }
    public List<SensorFilterDto> Sensors { get; set; }
}
