using Microsoft.EntityFrameworkCore;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Entities;
using TelemetryManager.Persistence.Mapping;

namespace TelemetryManager.Persistence.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly TelemetryContext _context;

    public TelemetryRepository(TelemetryContext context) => _context = context;

    public async Task AddPacketAsync(TelemetryPacket packet)
    {
        var entity = TelemetryPacketMapper.MapToEntity(packet);

        await _context.TelemetryPackets.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(
     DateTime? dateFrom = null,
     DateTime? dateTo = null,
     ushort? deviceId = null,
     SensorType? sensorType = null,
     byte? sensorId = null,
     int pageNumber = 1,
     int pageSize = 50)
    {
        var query = _context.TelemetryPackets
            .Include(p => p.ContentItems)
            .AsQueryable();

        // Применяем фильтры
        if (dateFrom.HasValue)
            query = query.Where(p => p.Time >= dateFrom);

        if (dateTo.HasValue)
            query = query.Where(p => p.Time < dateTo);

        if (deviceId.HasValue)
        {
            query = query.Where(p => p.DevId == deviceId);

            if (sensorType.HasValue)
                query = query.Where(p => p.SensorType == sensorType);

            if (sensorId.HasValue)
                query = query.Where(p => p.SensorSourceId == sensorId);
        }

        // Получаем общее количество записей перед пагинацией
        int totalRecords = await query.CountAsync();

        // Применяем пагинацию
        var entities = await query
            .OrderByDescending(p => p.Time)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Преобразуем в DTO
        var data = entities.Select(p => new TelemetryPacket(
            DateTimeOfSending: p.Time,
            DevId: p.DevId,
            SensorId: new SensorId(p.SensorType, p.SensorSourceId),
            Content: p.ContentItems.ToDictionary(i => i.Key, i => i.Value)
        )).ToList();

        return new PagedResponse<TelemetryPacket>(
            data: data,
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalRecords: totalRecords
        );
    }
}