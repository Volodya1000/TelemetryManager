using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Application.Mapping;

public static class FilterDtoMapper
{
    public static ContentTypeFilterDto ToFilterDto(this ContentDefinition contentDefinition)
    {
        if (contentDefinition == null)
            return null;

        return new ContentTypeFilterDto
        {
            Id = contentDefinition.TypeId,
            Name = contentDefinition.Name.Value
        };
    }

    public static SensorFilterDto ToFilterDto(this SensorProfile sensor, ContentTypeFilterDto contentType)
    {
        return new SensorFilterDto
        {
            SensorTypeId = sensor.Id.TypeId,
            SensorSourceId = sensor.Id.SourceId,
            Name = sensor.Name.Value,
            ContentType = contentType
        };
    }

    public static DeviceFilterDto ToFilterDto(this DeviceProfile device, List<SensorFilterDto> sensors)
    {
        return new DeviceFilterDto
        {
            DeviceId = device.DeviceId,
            Name = device.Name.Value,
            Sensors = sensors
        };
    }
}
