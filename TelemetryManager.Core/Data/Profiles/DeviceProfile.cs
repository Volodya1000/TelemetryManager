using System.Data;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public Name Name { get; init; }
    public DateTime? ActivationTime { get; private set; }

    private readonly List<SensorProfile> _sensors = new();
    public IReadOnlyCollection<SensorProfile> Sensors => _sensors.AsReadOnly();
    public IReadOnlyCollection<SensorId> SensorIds => _sensors.Select(s => s.Id).ToList().AsReadOnly();

    public DeviceProfile(ushort deviceId, Name name)
    {
        DeviceId = deviceId;
        Name = name;
    }

    public void AddSensor(SensorProfile newSensor)
    {
        if (_sensors.Any(s => s.Id == newSensor.Id))
            throw new ArgumentException(
                $"Sensor with ID {newSensor.Id} already exists",
                nameof(newSensor));

        _sensors.Add(newSensor);
    }

    /// <summary>
    /// Время активации изначально не известно и оно устанавливается только один раз по времени первого полученного пакета
    /// </summary>
    public void SetActivationTime(DateTime activationTime)
    {
        if (ActivationTime.HasValue)
            throw new InvalidOperationException(
                "Activation time has already been set and cannot be modified");

        ActivationTime = activationTime;
    }

    #region Parameter Configuration

    public void SetParameterInterval(SensorId sensorId, ParameterName name, Interval interval, DateTime timestamp)
    {
        var parameter = GetParameter(sensorId, name);
        parameter.SetInterval(interval, timestamp);
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
        var sensor = _sensors.FirstOrDefault(s => s.Id == sensorId);

        if (sensor is null)
            throw new KeyNotFoundException($"Sensor {sensorId} not found");

        return sensor;
    }

    public (bool isValid, double deviation, Interval currentInterval)
       CheckParameterValue(SensorId sensorId, ParameterName parameterName, double value,DateTime dateTime)
    {
        var parameter = GetParameter(sensorId, parameterName);
        var interval = parameter.GetIntervalForDate(dateTime);

        if (value < interval.Min)
        {
            return (false, value - interval.Min, interval);
        }

        if (value > interval.Max)
        {
            return (false, value - interval.Max, interval);
        }

        return (true, 0, interval);
    }
}