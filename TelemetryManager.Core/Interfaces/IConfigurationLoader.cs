using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Core.Interfaces;

public interface IConfigurationLoader
{
    SensorProfile Load(string configFilePath);
}
