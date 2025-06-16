using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Core.Interfaces;

public interface IConfigurationLoader
{
    DeviceProfile Load(string configFilePath);
}
