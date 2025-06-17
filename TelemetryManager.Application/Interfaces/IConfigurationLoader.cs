using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Application.Interfaces;

public interface IConfigurationLoader
{
    List<DeviceProfile> Load(string configFilePath);
}
