using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }

    public Name Name { get; init; }

    public DateTime? ActivationTime { get; private set; }



    private readonly Dictionary<SensorId,SensorProfile> _sensorsDict = new();

    public IReadOnlyList<SensorProfile> Sensors => _sensorsDict.Values.ToList();

    public IReadOnlyList<SensorId> SensorIds => _sensorsDict.Values.Select(s => s.Id).ToList();


    public DeviceProfile(ushort deviceId, Name name)
    {
        DeviceId = deviceId;
        Name = name;
    }

    public void AddSensor(SensorProfile newSensor)
    {
        if (_sensorsDict.ContainsKey(newSensor.Id))
            throw new ArgumentException(
                $"Sensor with TypeId={newSensor.TypeId} and SourceId={newSensor.SourceId} already exists",
                nameof(newSensor));

        _sensorsDict.Add(newSensor.Id, newSensor);

    }

    /// <summary>
    /// Время активации изначально не известно и оно устанавливается только один раз  по времени первого полученого пакета
    /// </summary>
    public void SetDeviceActivationTime(DateTime activationTime)
    {
        if (ActivationTime.HasValue)
            throw new InvalidOperationException(
                "Activation time has already been set and cannot be modified");
        ActivationTime =activationTime;
    }
}

