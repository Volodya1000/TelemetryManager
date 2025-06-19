using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public Name Name { get; init; }
    public DateTime? ActivationTime { get; private set; }
    private readonly Dictionary<SensorId,SensorProfile> _sensorsDict = new();
    public IReadOnlyCollection<SensorProfile> Sensors => _sensorsDict.Values;
    public IReadOnlyCollection<SensorId> SensorIds => _sensorsDict.Keys;

    public DeviceProfile(ushort deviceId, Name name)
    {
        DeviceId = deviceId;
        Name = name;
    }

    public void AddSensor(SensorProfile newSensor)
    {
        if (_sensorsDict.ContainsKey(newSensor.Id))
            throw new ArgumentException(
                $"Sensor with {newSensor.Id} already exists",
                nameof(newSensor));

        _sensorsDict.Add(newSensor.Id, newSensor);
    }
    

    /// <summary>
    /// Время активации изначально не известно и оно устанавливается только один раз  по времени первого полученого пакета
    /// </summary>
    public void SetActivationTime(DateTime activationTime)
    {
        if (ActivationTime.HasValue)
            throw new InvalidOperationException(
                "Activation time has already been set and cannot be modified");
        ActivationTime =activationTime;
    }

    #region Parameter Configuration
    public void SetParameterInterval(SensorId sensorId, ParameterName name, double min, double max)
    {
        var parameter = GetParameter(sensorId, name);
        parameter.SetInterval(min, max);
    }

    private SensorParameterProfile GetParameter(SensorId sensorId, ParameterName name)
    {
        return GetSensor(sensorId).GetParameter(name);
    }
    #endregion

    #region Connection Status Management
    public void MarkSensorConnected(SensorId sensorId, DateTime timestamp)
    {
        GetSensor(sensorId).MarkConnected(timestamp);
    }

    public void MarkSensorDisconnected(SensorId sensorId, DateTime timestamp)
    {
        GetSensor(sensorId).MarkDisconnected(timestamp);
    }

    public bool IsSensorConnectedAt(SensorId sensorId, DateTime timestamp)
    {
        return GetSensor(sensorId).IsConnectedAt(timestamp);
    }
    #endregion

    private SensorProfile GetSensor(SensorId sensorId)
    {
        if (!_sensorsDict.TryGetValue(sensorId, out var sensor))
            throw new KeyNotFoundException($"Sensor {sensorId} not found");

        return sensor;
    }
}

