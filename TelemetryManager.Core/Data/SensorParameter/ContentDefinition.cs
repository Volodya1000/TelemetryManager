namespace TelemetryManager.Core.Data.SensorParameter;

public class ContentDefinition
{
    public byte TypeId { get; }
    public string Name { get; }
    public IReadOnlyList<ParameterDefinition> Parameters { get; }
    public int TotalSizeBytes { get; }

    public ContentDefinition(byte typeId, string name, IEnumerable<ParameterDefinition> parameters)
    {
        TypeId = typeId;
        Name = name;
        Parameters = parameters.ToList().AsReadOnly();
        TotalSizeBytes = Parameters.Sum(p => p.ByteSize);
    }
}
