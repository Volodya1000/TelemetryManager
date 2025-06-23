using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Persistence.InMemoryRepositories;

public class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly List<DeviceProfile> _devices = new();

    public Task AddAsync(DeviceProfile deviceProfile)
    {
        if (deviceProfile == null)
            throw new ArgumentNullException(nameof(deviceProfile));

        if (_devices.Any(d => d.DeviceId == deviceProfile.DeviceId))
            throw new InvalidOperationException($"Device with ID {deviceProfile.DeviceId} already exists.");

        _devices.Add(deviceProfile);
        return Task.CompletedTask;
    }

    public Task<DeviceProfile> GetByIdAsync(ushort deviceId)
    {
        var device = _devices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device == null)
        {
            throw new KeyNotFoundException($"Device with ID {deviceId} not found.");
        }
        return Task.FromResult(device);
    }

    public Task UpdateAsync(DeviceProfile deviceProfile)
    {
        if (deviceProfile == null)
            throw new ArgumentNullException(nameof(deviceProfile));

        var existing = _devices.FirstOrDefault(d => d.DeviceId == deviceProfile.DeviceId);
        if (existing == null)
            throw new KeyNotFoundException($"Device with ID {deviceProfile.DeviceId} does not exist.");

        // Предполагаем, что DeviceProfile изменяемый и поддерживает обновление свойств.
        // Либо можно заменить элемент в списке:
        var index = _devices.IndexOf(existing);
        _devices[index] = deviceProfile;

        return Task.CompletedTask;
    }

    public Task<IEnumerable<DeviceProfile>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<DeviceProfile>>(_devices);
    }
}
