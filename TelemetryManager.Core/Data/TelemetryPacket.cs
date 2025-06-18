using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data;

public  record TelemetryPacket
(
    DateTime DateTimeOfSending,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);