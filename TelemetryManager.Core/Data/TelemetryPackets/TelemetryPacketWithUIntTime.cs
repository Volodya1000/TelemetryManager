using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.TelemetryPackets;

public record class TelemetryPacketWithUIntTime(
    uint Time,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);