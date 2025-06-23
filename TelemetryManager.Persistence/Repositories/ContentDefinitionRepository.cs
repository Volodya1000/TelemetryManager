using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Mapping;

namespace TelemetryManager.Persistence.Repositories;

/*
public class ContentDefinitionRepository : IContentDefinitionRepository
{
    private readonly TelemetryContext _context;

    public ContentDefinitionRepository(TelemetryContext context)
    {
        _context = context;
    }

    public async Task RegisterAsync(ContentDefinition definition)
    {
        if (await IsRegisteredAsync(definition.TypeId))
            throw new InvalidOperationException($"TypeId {definition.TypeId} уже зарегистрирован");

        var entity = ContentDefinitionMapper.ToEntity(definition);
        await _context.ContentDefinitions.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRegisteredAsync(byte typeId)
    {
        return await _context.ContentDefinitions
            .AnyAsync(cd => cd.TypeId == typeId);
    }

    public async Task<ContentDefinition?> GetDefinitionAsync(byte typeId)
    {
        var entity = await _context.ContentDefinitions
            .Include(cd => cd.Parameters)
            .FirstOrDefaultAsync(cd => cd.TypeId == typeId);

        return entity != null
            ? ContentDefinitionMapper.ToDomain(entity)
            :null;
    }
  

    public async Task<IEnumerable<ContentDefinition>> GetAllDefinitionsAsync()
    {
        var entities = await _context.ContentDefinitions
            .Include(cd => cd.Parameters)
            .ToListAsync();

        return entities.Select(ContentDefinitionMapper.ToDomain);
    }
}
*/