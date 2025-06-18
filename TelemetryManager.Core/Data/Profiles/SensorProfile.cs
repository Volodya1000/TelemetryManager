using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorId Id { get; }

    public SensorType TypeId => Id.TypeId;
    public byte SourceId => Id.SourceId;

    public Name Name { get; init; }
    public IReadOnlyList<SensorParameterProfile> Parameters { get; init; }

    public SensorProfile(SensorId id, Name name, IReadOnlyList<SensorParameterProfile> parameters)
    {
        var duplicateNames = parameters
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count > 0)
        {
            throw new ArgumentException($"Duplicate parameter names found: {string.Join(", ", duplicateNames)}");
        }

        Id = id;
        Name = name;
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

    }
}