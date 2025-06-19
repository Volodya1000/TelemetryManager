using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Application.Mapping;

public static class DomainModelsToDtosMapper
{
    public static DeviceProfileDto ToDto(this DeviceProfile domain)
    {
        return new DeviceProfileDto(
            DeviceId: domain.DeviceId,
            Name: domain.Name.Value, 
            ActivationTime: domain.ActivationTime,
            Sensors: domain.Sensors.Select(s => s.ToDto()).ToList()
        );
    }

    public static SensorProfileDto ToDto(this SensorProfile domain)
    {
        return new SensorProfileDto(
            SourceId: domain.SourceId,
            TypeId: domain.TypeId,
            Name: domain.Name.Value,
            Parameters: domain.Parameters.Select(p => p.ToDto()).ToList()
        );
    }

    public static SensorParameterProfileDto ToDto(this SensorParameterProfile domain)
    {
        return new SensorParameterProfileDto(
            ParameterName: domain.Name.Value, 
            Units: domain.Units,
            MinValue: domain.CurrentInterval.Min,
            MaxValue: domain.CurrentInterval.Max
        );
    }
}
