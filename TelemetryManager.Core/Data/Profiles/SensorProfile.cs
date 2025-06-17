using System.Text.Json.Serialization;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

//public class SensorProfile
//{
//    public SensorId SensorId { get; }
//    public string Name { get; }
//    public IReadOnlyList<SensorParameterProfile> Parameters { get; }

//    public SensorProfile(
//        SensorId SensorId,         
//        string Name,               
//        IReadOnlyList<SensorParameterProfile> Parameters) 
//    {
//        if (string.IsNullOrEmpty(Name)) throw new ArgumentNullException(nameof(Name));

//        this.SensorId = SensorId;
//        this.Name = Name;
//        this.Parameters = Parameters ?? throw new ArgumentNullException(nameof(Parameters));
//    }
//}

public class SensorProfile
{
    public SensorType TypeId { get; }

    public byte SourceId { get; }

    public required string Name { get; init; }
    public required IReadOnlyList<SensorParameterProfile> Parameters { get; init; }

    public SensorProfile( SensorType TypeId, byte SourceId, string Name, IReadOnlyList<SensorParameterProfile> Parameters)
    {
        this.TypeId = TypeId;
        this.SourceId = SourceId;
        this.Name = Name;
        this.Parameters = Parameters;

        var sensorType = TypeId;
    }
}