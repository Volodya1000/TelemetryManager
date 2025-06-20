using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Persistence.Entities.DeviceEntities;

namespace TelemetryManager.Persistence.Mapping;

public static class DeviceProfileMapper
{
    public static DeviceProfileEntity ToEntity(DeviceProfile domain)
    {
        return new DeviceProfileEntity
        {
            DeviceId = domain.DeviceId,
            Name = domain.Name.Value,
            ActivationTime = domain.ActivationTime,
            Sensors = domain.Sensors.Select(SensorProfileMapper.ToEntity).ToList()
        };
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

// SensorParameterProfileMapper.cs
public static class SensorParameterProfileMapper
{
    public static SensorParameterProfileEntity ToEntity(SensorParameterProfile domain)
    {
        return new SensorParameterProfileEntity
        {
            ParameterName = domain.Definition.Name.Value,
            Quantity = domain.Definition.Quantity,
            Unit = domain.Definition.Unit,
            DataTypeName = domain.Definition.DataType.FullName,
            CurrentMin = domain.CurrentInterval.Min,
            CurrentMax = domain.CurrentInterval.Max,
            IntervalHistory = domain.IntervalHistory
                .Select(h => new ParameterIntervalChangeRecordEntity
                {
                    ChangeTime = h.ChangeTime,
                    Min = h.Interval.Min,
                    Max = h.Interval.Max
                })
                .ToList()
        };
    }

    public static SensorParameterProfile ToDomain(SensorParameterProfileEntity entity)
    {
        var definition = new ParameterDefinition(
            new ParameterName(entity.ParameterName),
            entity.Quantity,
            entity.Unit,
            Type.GetType(entity.DataTypeName)
        );

        var parameter = new SensorParameterProfile(
            definition,
            entity.CurrentMin,
            entity.CurrentMax
        );

        // Восстановление истории интервалов
        foreach (var history in entity.IntervalHistory
            .OrderBy(h => h.ChangeTime)
            .Skip(1)) // Первый интервал уже установлен в конструкторе
        {
            parameter.SetInterval(history.Min, history.Max);
        }

        return parameter;
    }
}
