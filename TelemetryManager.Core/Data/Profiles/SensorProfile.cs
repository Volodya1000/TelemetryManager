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

    private readonly Dictionary<ParametrName, SensorParameterProfile> _parametersDict;
    public IReadOnlyList<SensorParameterProfile> Parameters => _parametersDict.Values.ToList();

    public SensorProfile(SensorId id, Name name, IReadOnlyList<SensorParameterProfile> parameters)
    {
        Id = id;
        Name = name;

        _parametersDict = parameters.ToDictionary(p => p.Name);
        if (_parametersDict.Count != parameters.Count)
            throw new ArgumentException("Duplicate parameter names found");
    }

    public SensorParameterProfile GetParameter(ParametrName name)
    {
        if (_parametersDict.TryGetValue(name, out var parameter))
            return parameter;

        throw new KeyNotFoundException($"Parameter '{name}' not found in sensor {Id}");
    }
}
