using System.Xml.Linq;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class DeviceProfile
{
    public ushort DeviceId { get; init; }
    public required string DeviceName { get; init; }
    public List<SensorProfile> Sensors { get; init; }

    public DeviceProfile(ushort deviceId, string deviceName, List<SensorProfile> sensors)
    {
        if (String.IsNullOrEmpty(deviceName)) throw new ArgumentNullException("name");
        DeviceId = deviceId;
        DeviceName = deviceName;
        Sensors= sensors;
    }
}

