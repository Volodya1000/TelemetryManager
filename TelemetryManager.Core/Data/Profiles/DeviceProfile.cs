using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public string Name { get; init; }
    private readonly Dictionary<SensorId,SensorProfile> _sensorsDict = new();

    public IReadOnlyList<SensorProfile> Sensors => _sensorsDict.Values.ToList();


    public DeviceProfile(ushort deviceId, string name)
    {
        DeviceId = deviceId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void AddSensor(SensorProfile newSensor)
    {
        if (_sensorsDict.ContainsKey(newSensor.Id))
            throw new ArgumentException(
                $"Sensor with TypeId={newSensor.TypeId} and SourceId={newSensor.SourceId} already exists",
                nameof(newSensor));

        _sensorsDict.Add(newSensor.Id, newSensor);

    }
}

