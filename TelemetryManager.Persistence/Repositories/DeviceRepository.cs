using Microsoft.EntityFrameworkCore;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Entities.DeviceEntities;
using TelemetryManager.Persistence.Mapping;

namespace TelemetryManager.Persistence.Repositories;

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
        _context.Entry(existing).CurrentValues.SetValues(new
        {
            entity.Name,
            entity.ActivationTime
        });

        // Синхронизация сенсоров
        UpdateSensorProfiles(existing, entity);
        await _context.SaveChangesAsync();
    }

    private void UpdateSensorProfiles(DeviceProfileEntity existing, DeviceProfileEntity updated)
    {
        // Удаление отсутствующих сенсоров
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
                // Добавляем новый сенсор с полной цепочкой
                _context.SensorProfiles.Add(updatedSensor);
                existing.Sensors.Add(updatedSensor);
            }
            else
            {
                // Обновление свойств сенсора
                _context.Entry(existingSensor).CurrentValues.SetValues(new
                {
                    updatedSensor.Name
                });

                // Синхронизация параметров
                UpdateSensorParameters(existingSensor, updatedSensor);

                // Синхронизация истории подключений
                UpdateConnectionHistory(existingSensor, updatedSensor);
            }
        }
    }

    private void UpdateSensorParameters(
        SensorProfileEntity existingSensor,
        SensorProfileEntity updatedSensor)
    {
        // Удаление отсутствующих параметров
        var paramsToDelete = existingSensor.Parameters
            .Where(ep => !updatedSensor.Parameters.Any(up => up.Id == ep.Id))
            .ToList();

        foreach (var param in paramsToDelete)
        {
            existingSensor.Parameters.Remove(param);
            _context.SensorParameters.Remove(param);
        }

        foreach (var updatedParam in updatedSensor.Parameters)
        {
            var existingParam = existingSensor.Parameters
                .FirstOrDefault(p => p.Id == updatedParam.Id);

            if (existingParam == null)
            {
                existingSensor.Parameters.Add(updatedParam);
            }
            else
            {
                // Обновление значений параметра
                _context.Entry(existingParam).CurrentValues.SetValues(new
                {
                    updatedParam.CurrentMin,
                    updatedParam.CurrentMax
                });

                // Синхронизация истории интервалов
                UpdateIntervalHistory(existingParam, updatedParam);
            }
        }
    }

    private void UpdateIntervalHistory(
     SensorParameterProfileEntity existingParam,
     SensorParameterProfileEntity updatedParam)
    {
        _context.IntervalHistory.RemoveRange(existingParam.IntervalHistory);
        existingParam.IntervalHistory.Clear();

        foreach (var history in updatedParam.IntervalHistory)
        {
            // Устанавливаем связь с параметром
            history.Parameter = existingParam;
            history.ParameterId = existingParam.Id;
            existingParam.IntervalHistory.Add(history);
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