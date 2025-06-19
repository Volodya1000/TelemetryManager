using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.TelemetryPackets;

public  record TelemetryPacket
(
    DateTime DateTimeOfSending,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);