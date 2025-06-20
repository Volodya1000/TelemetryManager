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

        // Обновляем корневую сущность
        _context.Entry(existing).CurrentValues.SetValues(entity);

        // Обновляем дочерние коллекции (сенсоры, параметры и т.д.)
        UpdateChildCollections(existing, entity);

        await _context.SaveChangesAsync();
    }

    private void UpdateChildCollections(DeviceProfileEntity existing, DeviceProfileEntity updated)
    {
        // Обновляем корневые свойства устройства
        _context.Entry(existing).CurrentValues.SetValues(updated);

        // Обрабатываем вложенные коллекции
        UpdateSensorsCollection(existing, existing.Sensors, updated.Sensors);
    }

    private void UpdateSensorsCollection(
        DeviceProfileEntity device,
        ICollection<SensorProfileEntity> existingSensors,
        ICollection<SensorProfileEntity> updatedSensors)
    {
        // Удаляем отсутствующие сенсоры
        foreach (var existingSensor in existingSensors.ToList())
        {
            if (!updatedSensors.Any(us => us.Id == existingSensor.Id && existingSensor.Id != 0))
            {
                // Явное удаление для каскадного удаления зависимостей
                _context.Remove(existingSensor);
            }
        }

        foreach (var updatedSensor in updatedSensors)
        {
            var existingSensor = existingSensors
                .FirstOrDefault(s => s.Id == updatedSensor.Id && updatedSensor.Id != 0);

            if (existingSensor != null)
            {
                // Обновляем свойства сенсора
                _context.Entry(existingSensor).CurrentValues.SetValues(updatedSensor);

                // Важно: сохраняем ссылку на устройство
                existingSensor.Device = device;
                existingSensor.DeviceId = device.DeviceId;

                // Обрабатываем вложенные коллекции
                UpdateConnectionHistory(existingSensor, existingSensor.ConnectionHistory, updatedSensor.ConnectionHistory);
                UpdateSensorParameters(existingSensor, existingSensor.Parameters, updatedSensor.Parameters);
            }
            else
            {
                // Добавляем новый сенсор с привязкой к устройству
                updatedSensor.Device = device;
                updatedSensor.DeviceId = device.DeviceId;
                existingSensors.Add(updatedSensor);
            }
        }
    }

    private void UpdateSensorParameters(
        SensorProfileEntity sensor,
        ICollection<SensorParameterProfileEntity> existingParameters,
        ICollection<SensorParameterProfileEntity> updatedParameters)
    {
        foreach (var existingParam in existingParameters.ToList())
        {
            if (!updatedParameters.Any(up => up.Id == existingParam.Id && existingParam.Id != 0))
            {
                _context.Remove(existingParam);
            }
        }

        foreach (var updatedParam in updatedParameters)
        {
            var existingParam = existingParameters
                .FirstOrDefault(p => p.Id == updatedParam.Id && updatedParam.Id != 0);

            if (existingParam != null)
            {
                _context.Entry(existingParam).CurrentValues.SetValues(updatedParam);
                existingParam.Sensor = sensor;
                existingParam.SensorId = sensor.Id;

                UpdateIntervalHistory(existingParam, existingParam.IntervalHistory, updatedParam.IntervalHistory);
            }
            else
            {
                updatedParam.Sensor = sensor;
                updatedParam.SensorId = sensor.Id;
                existingParameters.Add(updatedParam);
            }
        }
    }

    private void UpdateConnectionHistory(
        SensorProfileEntity sensor,
        ICollection<SensorConnectionHistoryRecordEntity> existingHistory,
        ICollection<SensorConnectionHistoryRecordEntity> updatedHistory)
    {
        foreach (var existingRecord in existingHistory.ToList())
        {
            if (!updatedHistory.Any(uh => uh.Id == existingRecord.Id && existingRecord.Id != 0))
            {
                _context.Remove(existingRecord);
            }
        }

        foreach (var updatedRecord in updatedHistory)
        {
            var existingRecord = existingHistory
                .FirstOrDefault(h => h.Id == updatedRecord.Id && updatedRecord.Id != 0);

            if (existingRecord != null)
            {
                _context.Entry(existingRecord).CurrentValues.SetValues(updatedRecord);
                existingRecord.Sensor = sensor;
                existingRecord.SensorId = sensor.Id;
            }
            else
            {
                updatedRecord.Sensor = sensor;
                updatedRecord.SensorId = sensor.Id;
                existingHistory.Add(updatedRecord);
            }
        }
    }

    private void UpdateIntervalHistory(
        SensorParameterProfileEntity parameter,
        ICollection<ParameterIntervalChangeRecordEntity> existingHistory,
        ICollection<ParameterIntervalChangeRecordEntity> updatedHistory)
    {
        foreach (var existingRecord in existingHistory.ToList())
        {
            if (!updatedHistory.Any(uh => uh.Id == existingRecord.Id && existingRecord.Id != 0))
            {
                _context.Remove(existingRecord);
            }
        }

        foreach (var updatedRecord in updatedHistory)
        {
            var existingRecord = existingHistory
                .FirstOrDefault(h => h.Id == updatedRecord.Id && updatedRecord.Id != 0);

            if (existingRecord != null)
            {
                _context.Entry(existingRecord).CurrentValues.SetValues(updatedRecord);
                existingRecord.Parameter = parameter;
                existingRecord.ParameterId = parameter.Id;
            }
            else
            {
                updatedRecord.Parameter = parameter;
                updatedRecord.ParameterId = parameter.Id;
                existingHistory.Add(updatedRecord);
            }
        }
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
}