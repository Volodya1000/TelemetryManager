using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorId SensorId { get; init; }

    public required string Name { get; init; }

    public required List<SensorParameterProfile> SensorParametrs { get; init; }
}
