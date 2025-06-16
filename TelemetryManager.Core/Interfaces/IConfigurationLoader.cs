using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Core.Interfaces;

public interface IConfigurationLoader
{
    List<DeviceProfile> Load(string configFilePath);
}
