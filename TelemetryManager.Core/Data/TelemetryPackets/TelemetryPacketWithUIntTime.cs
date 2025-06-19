using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.TelemetryPackets;

public record class TelemetryPacketWithUIntTime(
    uint Time,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);