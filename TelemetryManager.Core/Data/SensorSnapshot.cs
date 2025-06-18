using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data;

public record SensorSnapshot(
    SensorId SensorId, 
    Name SensorName, 
    DateTime SnapshotDateTime,
    IReadOnlyList<SensorParametrSnapshot> Parameters);
