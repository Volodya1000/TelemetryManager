


using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core;

public class TelemtryManagerFacade
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConfigurationValidator _configurationValidator;

    public TelemtryManagerFacade(
        IConfigurationLoader configurationLoader,
        IConfigurationValidator configurationValidator)
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
    }
    public void LoadConfiguration(string configFilePath)
    {
        DeviceProfile deviceProfile = _configurationLoader.Load(configFilePath);
        _configurationValidator.Validate(deviceProfile);
    }

}
