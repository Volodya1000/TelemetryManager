using Microsoft.Extensions.Logging;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Infrastructure.Parsing;

public class ParserEnvironment
{
    public Stream Stream { get; }
    public IDictionary<ushort, IReadOnlyCollection<SensorId>> AvailableSensors { get; }
    public IContentTypeParser ContentParser { get; }
    public IContentDefinitionRepository DefinitionRepo { get; }
    public ILogger Logger { get; }

    public ParserEnvironment(
        Stream stream,
        IDictionary<ushort, IReadOnlyCollection<SensorId>> sensors,
        IContentTypeParser contentParser,
        IContentDefinitionRepository defRepo,
        ILogger logger)
    {
        Stream = stream;
        AvailableSensors = sensors;
        ContentParser = contentParser;
        DefinitionRepo = defRepo;
        Logger = logger;
    }
}
