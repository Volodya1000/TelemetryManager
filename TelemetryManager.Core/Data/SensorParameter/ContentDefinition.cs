using TelemetryManager.Core.Data.ValueObjects;

namespace TelemetryManager.Core.Data.SensorParameter;

public class ContentDefinition
{
    public byte TypeId { get; }
    public Name Name { get; }
    public IReadOnlyList<ParameterDefinition> Parameters { get; }
    public int TotalSizeBytes { get; }

    public ContentDefinition(byte typeId, Name name, IEnumerable<ParameterDefinition> parameters)
    {
        if (parameters.GroupBy(p => p.Name).Any(g => g.Count() > 1))
            throw new ArgumentException("Duplicate parameter names");

        TypeId = typeId;
        Name = name;
        Parameters = parameters.ToList().AsReadOnly();
        TotalSizeBytes = Parameters.Sum(p => p.ByteSize);
    }
}
