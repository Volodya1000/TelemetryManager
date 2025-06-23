using Microsoft.EntityFrameworkCore;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Entities.DeviceEntities;
using TelemetryManager.Persistence.Mapping;

namespace TelemetryManager.Persistence.Repositories;

/*
public class DeviceRepository : IDeviceRepository
{
    private readonly TelemetryContext _context;

    public DeviceRepository(TelemetryContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeviceProfile deviceProfile)
    {
        var entity = DeviceProfileMapper.ToEntity(deviceProfile);
        await _context.DeviceProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<DeviceProfile> GetByIdAsync(ushort deviceId)
    {
        var entity = await _context.DeviceProfiles
            .AsNoTracking()
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        return entity != null ? DeviceProfileMapper.ToDomain(entity) : null;
    }

    public async Task<IEnumerable<DeviceProfile>> GetAllAsync()
    {
        var entities = await _context.DeviceProfiles
            .AsNoTracking()
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .ToListAsync();

        return entities.Select(DeviceProfileMapper.ToDomain);
    }





    public async Task UpdateAsync(DeviceProfile deviceProfile)
    {
        var entity = DeviceProfileMapper.ToEntity(deviceProfile);
        var existing = await _context.DeviceProfiles
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .FirstOrDefaultAsync(d => d.DeviceId == deviceProfile.DeviceId);

        if (existing == null)
        {
            await AddAsync(deviceProfile);
            return;
        }

        // Обновление корневых свойств
        existing.Name = entity.Name;
        existing.ActivationTime = entity.ActivationTime;

        // Синхронизация сенсоров
        UpdateSensorProfiles(existing, entity);
        await _context.SaveChangesAsync();
    }

    private void UpdateSensorProfiles(DeviceProfileEntity existing, DeviceProfileEntity updated)
    {
        // Удаление отсутствующих сенсоров (по составному ключу)
        var sensorsToDelete = existing.Sensors
            .Where(es => !updated.Sensors.Any(us =>
                us.DeviceId == es.DeviceId &&
                us.TypeId == es.TypeId &&
                us.SourceId == es.SourceId))
            .ToList();

        foreach (var sensor in sensorsToDelete)
        {
            existing.Sensors.Remove(sensor);
            _context.SensorProfiles.Remove(sensor);
        }

        foreach (var updatedSensor in updated.Sensors)
        {
            var existingSensor = existing.Sensors
                .FirstOrDefault(s =>
                    s.DeviceId == updatedSensor.DeviceId &&
                    s.TypeId == updatedSensor.TypeId &&
                    s.SourceId == updatedSensor.SourceId);

            if (existingSensor == null)
            {
                // при добавлении новых сразу все ключи заполнены
                _context.SensorProfiles.Add(updatedSensor);
                existing.Sensors.Add(updatedSensor);
            }
            else
            {
                existingSensor.Name = updatedSensor.Name;
                UpdateSensorParameters(existingSensor, updatedSensor);
                UpdateConnectionHistory(existingSensor, updatedSensor);
            }
        }
    }

    private void UpdateSensorParameters(
     SensorProfileEntity existingSensor,
     SensorProfileEntity updatedSensor)
    {
        // Удаляем
        var toRemove = existingSensor.Parameters
            .Where(ep => !updatedSensor.Parameters.Any(up =>
                up.ParameterName == ep.ParameterName &&
                up.DeviceId == ep.DeviceId &&
                up.TypeId == ep.TypeId &&
                up.SourceId == ep.SourceId))
            .ToList();

        foreach (var p in toRemove)
        {
            existingSensor.Parameters.Remove(p);
            _context.SensorParameters.Remove(p);
        }

        // Добавляем/обновляем
        foreach (var up in updatedSensor.Parameters)
        {
            var exists = existingSensor.Parameters.FirstOrDefault(ep =>
                ep.ParameterName == up.ParameterName);

            if (exists == null)
            {
                existingSensor.Parameters.Add(up);
            }
            else
            {
                exists.CurrentMin = up.CurrentMin;
                exists.CurrentMax = up.CurrentMax;
                UpdateIntervalHistory(exists, up);
            }
        }
    }

    private void UpdateIntervalHistory(
        SensorParameterProfileEntity existingParam,
        SensorParameterProfileEntity updatedParam)
    {
        _context.IntervalHistory.RemoveRange(existingParam.IntervalHistory);
        existingParam.IntervalHistory.Clear();

        foreach (var hist in updatedParam.IntervalHistory)
        {
            // все четыре поля ключа уже заполнены в mapper-е
            hist.Parameter = existingParam;
            existingParam.IntervalHistory.Add(hist);
        }
    }



    private void UpdateConnectionHistory(
     SensorProfileEntity existingSensor,
     SensorProfileEntity updatedSensor)
    {
        _context.ConnectionHistory.RemoveRange(existingSensor.ConnectionHistory);
        existingSensor.ConnectionHistory.Clear();

        foreach (var history in updatedSensor.ConnectionHistory)
        {
            // Устанавливаем составные ключи
            history.DeviceId = existingSensor.DeviceId;
            history.TypeId = existingSensor.TypeId;
            history.SourceId = existingSensor.SourceId;
            history.Sensor = existingSensor;
            existingSensor.ConnectionHistory.Add(history);
        }
    }
}
*/