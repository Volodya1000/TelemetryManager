using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorType TypeId { get; }

    public byte SourceId { get; }

    public string Name { get; init; }
    public IReadOnlyList<SensorParameterProfile> Parameters { get; init; }

    public SensorProfile( SensorType TypeId, byte SourceId, string Name, IReadOnlyList<SensorParameterProfile> Parameters)
    {
        this.TypeId = TypeId;
        this.SourceId = SourceId;
        this.Name = Name;
        this.Parameters =Parameters;// new List<SensorParameterProfile>();

        var sensorType = TypeId;
    }
}