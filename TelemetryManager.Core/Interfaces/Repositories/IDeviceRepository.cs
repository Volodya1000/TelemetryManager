using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Core.Interfaces.Repositories;

public interface IDeviceRepository
{
    Task AddAsync(DeviceProfile deviceProfile);
    Task<DeviceProfile> GetByIdAsync(ushort deviceId);
    Task UpdateAsync(DeviceProfile deviceProfile);
    Task<IEnumerable<DeviceProfile>> GetAllAsync();
}
