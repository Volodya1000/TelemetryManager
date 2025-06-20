using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Entities.DeviceEntities;

public class DeviceProfileEntity
{
    public ushort DeviceId { get; set; }
    public string Name { get; set; }
    public DateTime? ActivationTime { get; set; }
    public List<SensorProfileEntity> Sensors { get; set; } = new();
}

public class SensorProfileEntity
{
    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string Name { get; set; }
    public DeviceProfileEntity Device { get; set; }
    public List<SensorConnectionHistoryRecordEntity> ConnectionHistory { get; set; } = new();
    public List<SensorParameterProfileEntity> Parameters { get; set; } = new();
}


public class SensorParameterProfileEntity
{
    // PK: Device + Sensor + ParameterName
    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }

    // теперь обязательный ключ
    public string ParameterName { get; set; }

    public double CurrentMin { get; set; }
    public double CurrentMax { get; set; }

    public SensorProfileEntity Sensor { get; set; }
    public ParameterDefinitionEntity ParameterDefinition { get; set; }
    public List<ParameterIntervalChangeRecordEntity> IntervalHistory { get; set; }
        = new();
}

public class SensorConnectionHistoryRecordEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsConnected { get; set; }

    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }

    public SensorProfileEntity Sensor { get; set; }
}

public class ParameterIntervalChangeRecordEntity
{
    public int Id { get; set; }

    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string ParameterName { get; set; }

    public DateTime ChangeTime { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }

    public SensorParameterProfileEntity Parameter { get; set; }
}