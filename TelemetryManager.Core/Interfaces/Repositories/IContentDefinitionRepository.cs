using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Core.Interfaces.Repositories;

public interface IContentDefinitionRepository
{
    Task RegisterAsync(ContentDefinition definition);
    Task<bool> IsRegisteredAsync(byte typeId);
    Task<ContentDefinition?> GetDefinitionAsync(byte typeId);
    Task<IEnumerable<ContentDefinition>> GetAllDefinitionsAsync();
}