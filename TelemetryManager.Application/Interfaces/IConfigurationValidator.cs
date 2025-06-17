using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Application.Interfaces;

public interface IConfigurationValidator
{
    public void Validate(List<DeviceProfile> deviceProfile);
}

