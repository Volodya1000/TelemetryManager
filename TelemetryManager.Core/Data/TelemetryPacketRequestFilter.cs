using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data;

public record TelemetryPacketRequestFilter(
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    ushort? DeviceId = null,
    SensorId? SensorId = null,
    int PageNumber = 1,
    int PageSize = 50
);
