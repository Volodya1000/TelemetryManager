using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public Name Name { get; init; }
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
}

