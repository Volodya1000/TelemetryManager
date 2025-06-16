using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Core.Interfaces;

public interface IConfigurationValidator
{
    public void Validate(List<DeviceProfile> deviceProfile);
}

