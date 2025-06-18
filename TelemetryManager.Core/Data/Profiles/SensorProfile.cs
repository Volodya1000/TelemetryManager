using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorId Id { get; }

    public SensorType TypeId => Id.TypeId;
    public byte SourceId => Id.SourceId;

    public string Name { get; init; }
    public IReadOnlyList<SensorParameterProfile> Parameters { get; init; }

    public SensorProfile(SensorId id, string name, IReadOnlyList<SensorParameterProfile> parameters)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}