using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Entities.DeviceEntities;

internal sealed class  DeviceProfileEntity : DbEntity<ushort>
{
    public required string Name { get; set; }
    public DateTime? ActivationTime { get; set; }
    public required ICollection<SensorProfileEntity> Sensors { get; set; }
}

internal sealed class SensorProfileEntity
{
    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }

    public required string Name { get; set; }
    public required DeviceProfileEntity Device { get; set; }
    public required ICollection<SensorConnectionHistoryRecordEntity> ConnectionHistory { get; set; } 
    public required ICollection<SensorParameterProfileEntity> Parameters { get; set; } 
}


public class SensorParameterProfileEntity
{
    public ushort DeviceId { get; set; }
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }

    public required string ParameterName { get; set; }

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