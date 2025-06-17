using System.Xml.Linq;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public required string Name { get; init; }
    public required List<SensorProfile> Sensors { get; init; }

    public DeviceProfile(ushort deviceId, string name, List<SensorProfile> sensors)
    {
        DeviceId = deviceId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Sensors = sensors ?? throw new ArgumentNullException(nameof(sensors));
    }
}

