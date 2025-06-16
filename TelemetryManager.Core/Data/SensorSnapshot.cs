using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data;

public record SensorSnapshot(
    SensorId SensorId, 
    string SensorName, 
    DateTime TimeStamp,
    IReadOnlyList<SensorParametrSnapshot> Parameters);
