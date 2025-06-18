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

    public void RemoveSensor(SensorId sensorId)
    {
        if (_sensorsDict.ContainsKey(sensorId))
            throw new ArgumentException(
               $"Device with DeviceId={DeviceId} dont contains sensor with {sensorId}",
                nameof(sensorId));

        _sensorsDict.Remove(sensorId);
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

    public void SetParameterInterval(SensorId sensorId, ParametrName name, double min, double max)
    {
        var parameter = GetParameter(sensorId, name);
        parameter.SetInterval(min, max);
    }

    public void SetParameterMinValue(SensorId sensorId, ParametrName name, double min)
    {
        var parameter = GetParameter(sensorId, name);
        parameter.SetMinValue(min);
    }

    public void SetParameterMaxValue(SensorId sensorId, ParametrName name, double max)
    {
        var parameter = GetParameter(sensorId, name);
        parameter.SetMaxValue(max);
    }

    private SensorParameterProfile GetParameter(SensorId sensorId, ParametrName name)
    {
        if (!_sensorsDict.TryGetValue(sensorId, out var sensor))
            throw new KeyNotFoundException($"Sensor {sensorId} not found in device {DeviceId}");

        return sensor.GetParameter(name);
    }
}

