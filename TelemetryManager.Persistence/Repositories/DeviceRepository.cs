using Microsoft.EntityFrameworkCore;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Mapping;

namespace TelemetryManager.Persistence.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly TelemetryContext _context;

    public DeviceRepository(TelemetryContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeviceProfile deviceProfile)
    {
        var entity = DeviceProfileMapper.ToEntity(deviceProfile);
        await _context.DeviceProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<DeviceProfile> GetByIdAsync(ushort deviceId)
    {
        var entity = await _context.DeviceProfiles
            .AsNoTracking()
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        return entity != null ? DeviceProfileMapper.ToDomain(entity) : null;
    }

    public async Task UpdateAsync(DeviceProfile deviceProfile)
    {
        var existing = await _context.DeviceProfiles
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .FirstOrDefaultAsync(d => d.DeviceId == deviceProfile.DeviceId);

        if (existing != null)
        {
            _context.DeviceProfiles.Remove(existing);
            await _context.SaveChangesAsync();
        }

        await AddAsync(deviceProfile);
    }

    public async Task<IEnumerable<DeviceProfile>> GetAllAsync()
    {
        var entities = await _context.DeviceProfiles
            .AsNoTracking()
            .Include(d => d.Sensors)
                .ThenInclude(s => s.ConnectionHistory)
            .Include(d => d.Sensors)
                .ThenInclude(s => s.Parameters)
                    .ThenInclude(p => p.IntervalHistory)
            .ToListAsync();

        return entities.Select(DeviceProfileMapper.ToDomain);
    }
}