using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Persistence.InMemoryRepositories;

public class InMemoryContentDefinitionRepository : IContentDefinitionRepository
{
    private readonly List<ContentDefinition> _definitions = new();

    public Task RegisterAsync(ContentDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        if (_definitions.Any(d => d.TypeId == definition.TypeId))
            throw new InvalidOperationException($"Definition with type ID {definition.TypeId} is already registered.");

        _definitions.Add(definition);
        return Task.CompletedTask;
    }

    public Task<bool> IsRegisteredAsync(byte typeId)
    {
        return Task.FromResult(_definitions.Any(d => d.TypeId == typeId));
    }

    public Task<ContentDefinition?> GetDefinitionAsync(byte typeId)
    {
        var definition = _definitions.FirstOrDefault(d => d.TypeId == typeId);
        return Task.FromResult<ContentDefinition?>(definition);
    }

    public Task<IEnumerable<ContentDefinition>> GetAllDefinitionsAsync()
    {
        return Task.FromResult<IEnumerable<ContentDefinition>>(_definitions);
    }
}