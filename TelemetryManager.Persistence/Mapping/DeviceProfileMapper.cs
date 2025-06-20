using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Persistence.Entities.ContentEntities;
using TelemetryManager.Persistence.Entities.DeviceEntities;

namespace TelemetryManager.Persistence.Mapping;

public static class DeviceProfileMapper
{
    public static DeviceProfileEntity ToEntity(DeviceProfile deviceProfile)
    {
        var entity = new DeviceProfileEntity
        {
            DeviceId = deviceProfile.DeviceId,
            Name = deviceProfile.Name.Value,
            ActivationTime = deviceProfile.ActivationTime,
            Sensors = new List<SensorProfileEntity>()
        };

        foreach (var sensor in deviceProfile.Sensors)
        {
            var sensorEntity = new SensorProfileEntity
            {
                DeviceId = deviceProfile.DeviceId,
                TypeId = sensor.Id.TypeId,
                SourceId = sensor.Id.SourceId,
                Name = sensor.Name.Value,
                Parameters = new List<SensorParameterProfileEntity>(),
                ConnectionHistory = sensor.ConnectionHistory
                    .Select(h => new SensorConnectionHistoryRecordEntity
                    {
                        Timestamp = h.Timestamp,
                        IsConnected = h.IsConnected,
                        DeviceId = deviceProfile.DeviceId,
                        TypeId = sensor.Id.TypeId,
                        SourceId = sensor.Id.SourceId
                    }).ToList()
            };

            foreach (var parameter in sensor.Parameters)
            {
                var paramEntity = new SensorParameterProfileEntity
                {
                    DeviceId = deviceProfile.DeviceId,
                    TypeId = sensor.Id.TypeId,
                    SourceId = sensor.Id.SourceId,

                    // Обязательно проставляем ParameterName
                    ParameterName = parameter.Definition.Name.Value,

                    CurrentMin = parameter.CurrentInterval.Min,
                    CurrentMax = parameter.CurrentInterval.Max,

                    IntervalHistory = parameter.IntervalHistory
                        .Select(h => new ParameterIntervalChangeRecordEntity
                        {
                            ChangeTime = h.ChangeTime,
                            Min = h.Interval.Min,
                            Max = h.Interval.Max,
                            DeviceId = deviceProfile.DeviceId,
                            TypeId = sensor.Id.TypeId,
                            SourceId = sensor.Id.SourceId,
                            ParameterName = parameter.Definition.Name.Value
                        }).ToList()
                };

                sensorEntity.Parameters.Add(paramEntity);
            }

            entity.Sensors.Add(sensorEntity);
        }

        return entity;
    }

    public static DeviceProfile ToDomain(DeviceProfileEntity entity)
    {
        var device = new DeviceProfile(
            (ushort)entity.DeviceId,
            new Name(entity.Name)
        );

        if (entity.ActivationTime.HasValue)
        {
            device.SetActivationTime(entity.ActivationTime.Value);
        }

        foreach (var sensorEntity in entity.Sensors)
        {
            var sensor = SensorProfileMapper.ToDomain(sensorEntity);
            device.AddSensor(sensor);
        }

        return device;
    }
}

// SensorProfileMapper.cs
public static class SensorProfileMapper
{
    public static SensorProfileEntity ToEntity(SensorProfile domain)
    {
        return new SensorProfileEntity
        {
            TypeId = domain.Id.TypeId,
            SourceId = domain.Id.SourceId,
            Name = domain.Name.Value,
            ConnectionHistory = domain.ConnectionHistory
                .Select(h => new SensorConnectionHistoryRecordEntity
                {
                    Timestamp = h.Timestamp,
                    IsConnected = h.IsConnected
                })
                .ToList(),
            Parameters = domain.Parameters
                .Select(SensorParameterProfileMapper.ToEntity)
                .ToList()
        };
    }

    public static SensorProfile ToDomain(SensorProfileEntity entity)
    {
        var sensorId = new SensorId(entity.TypeId, entity.SourceId);
        var parameters = entity.Parameters
            .Select(SensorParameterProfileMapper.ToDomain)
            .ToList();

        var sensor = new SensorProfile(
            sensorId,
            new Name(entity.Name),
            parameters
        );

        // Восстановление истории соединений
        foreach (var history in entity.ConnectionHistory
            .OrderBy(h => h.Timestamp))
        {
            if (history.IsConnected) sensor.MarkConnected(history.Timestamp);
            else sensor.MarkDisconnected(history.Timestamp);
        }

        return sensor;
    }
}


public static class SensorParameterProfileMapper
{
    public static SensorParameterProfileEntity ToEntity(SensorParameterProfile domain)
    {
        return null;
    }

    public static SensorParameterProfile ToDomain(SensorParameterProfileEntity entity)
    {
        
        return null;
    }
}
