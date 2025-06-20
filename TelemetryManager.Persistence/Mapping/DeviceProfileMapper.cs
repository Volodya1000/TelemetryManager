using System.ComponentModel.DataAnnotations;
using System.Linq;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;
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
        if (entity == null) return null;

        var device = new DeviceProfile(
                    (ushort)entity.DeviceId,
                    new Name(entity.Name)
                );

        if (entity.ActivationTime.HasValue)
        {
            device.SetActivationTime(entity.ActivationTime.Value);
        }

        // Маппинг каждого сенсора
        foreach (var sensorEntity in entity.Sensors)
        {
            // Маппинг параметров сенсора
            var parameters = sensorEntity.Parameters.Select(pEntity =>
            {
            // Определение параметра
            var definition = new ParameterDefinition(
                new ParameterName(pEntity.ParameterName),
                "a", // временно: использовать текущее значение как Quantity, если нужно
                "a", // аналогично
                typeof(string)
                );

                // Текущее значение интервала
                var currentInterval = new 
                Interval(
                    pEntity.CurrentMin,
                    pEntity.CurrentMax
                );

                // Создаём профиль параметра
                var parameterProfile = new SensorParameterProfile(
                    definition,
                    pEntity.CurrentMin,
                    pEntity.CurrentMax
                );

                // История интервалов
                foreach (var hist in pEntity.IntervalHistory.OrderBy(h => h.ChangeTime))
                {
                    //parameterProfile.AddIntervalHistory(
                    //    new ParameterIntervalChangeRecord(
                    //        hist.ChangeTime,
                    //        new Interval(hist.Min, hist.Max)
                    //    )
                    //);
                }

                return parameterProfile;
            }).ToList();

            // Создаём профиль сенсора
            var sensorDomain = new SensorProfile(
                new SensorId(sensorEntity.TypeId, sensorEntity.SourceId),
                new Name(sensorEntity.Name),
                parameters
            );

            // История подключений
            foreach (var conn in sensorEntity.ConnectionHistory.OrderBy(c => c.Timestamp))
            {
                if (conn.IsConnected)
                    sensorDomain.MarkConnected(conn.Timestamp);
                else
                    sensorDomain.MarkDisconnected(conn.Timestamp);
            }

            device.AddSensor(sensorDomain);
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
