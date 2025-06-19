using TelemetryManager.Core.Enums;

namespace TelemetryManager.Application.Requests;

public record TelemetryPacketFilterRequest(
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    ushort? DeviceId = null,
    SensorType ? SensorType=null,
    byte? SensorId = null,
    int PageNumber = 1,
    int PageSize = 50
);
