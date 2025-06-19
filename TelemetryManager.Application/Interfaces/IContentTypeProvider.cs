using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Application.Interfaces;

public interface IContentTypeProvider
{
    void Register(ContentDefinition definition);

    bool IsRegistered(byte typeId);

    ContentDefinition GetDefinition(byte typeId);

    string GetName(byte typeId);

    IEnumerable<ContentDefinition> AllDefinitions { get; }
}