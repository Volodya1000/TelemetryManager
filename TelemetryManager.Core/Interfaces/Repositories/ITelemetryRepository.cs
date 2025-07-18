﻿using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;

namespace TelemetryManager.Core.Interfaces.Repositories;

public interface ITelemetryRepository
{
    Task AddPacketAsync(TelemetryPacket packet);
    Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        ushort? deviceId = null,
        byte? sensorType = null,
        byte? sensorId = null,
        int pageNumber = 1,
        int pageSize = 50);
}