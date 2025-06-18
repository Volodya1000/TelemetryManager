using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data;

public record class TelemetryPacket(
    uint Time,
    ushort DevId,
    SensorId SensorId,
    IReadOnlyDictionary<string, double> Content
);