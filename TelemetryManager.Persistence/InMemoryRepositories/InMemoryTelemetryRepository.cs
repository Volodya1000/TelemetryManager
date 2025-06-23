using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Persistence.InMemoryRepositories;

public class InMemoryTelemetryRepository : ITelemetryRepository
{
    private readonly List<TelemetryPacket> _packets = new();

    public Task AddPacketAsync(TelemetryPacket packet)
    {
        if (packet == null)
            throw new ArgumentNullException(nameof(packet));

        _packets.Add(packet);
        return Task.CompletedTask;
    }

    public Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        ushort? deviceId = null,
        byte? sensorType = null,
        byte? sensorId = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        IQueryable<TelemetryPacket> query = _packets.AsQueryable();

        // Фильтрация по дате
        if (dateFrom.HasValue)
            query = query.Where(p => p.DateTimeOfSending >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(p => p.DateTimeOfSending <= dateTo.Value);

        // Фильтрация по устройству
        if (deviceId.HasValue)
            query = query.Where(p => p.DevId == deviceId.Value);

        // Фильтрация по типу и ID сенсора
        if (sensorType.HasValue)
            query = query.Where(p => p.SensorId.TypeId == sensorType.Value);
        if (sensorId.HasValue)
            query = query.Where(p => p.SensorId.SourceId == sensorId.Value);

        // Пагинация
        var totalItems = query.Count();
        var items = query
            .OrderByDescending(p => p.DateTimeOfSending)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new PagedResponse<TelemetryPacket>(
            items,
            totalItems,
            pageNumber,
            pageSize
        );

        return Task.FromResult(response);
    }
}