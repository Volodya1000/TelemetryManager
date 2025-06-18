using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data;

public  record TelemetryPacketWithDate
(
    DateTime DateTimeOfSending,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);