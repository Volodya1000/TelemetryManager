using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure.Parsing.ParserStates;

namespace TelemetryManager.Infrastructure.Parsing;


public class PacketStreamParser : IPacketStreamParser
{
    private readonly ILogger<PacketStreamParser> _logger;
    private readonly IContentTypeParser _contentParser;
    private readonly IContentDefinitionRepository _defRepo;

    public const int MaxPacketSize = 1024;

    public PacketStreamParser(
        IContentTypeParser contentParser,
        IContentDefinitionRepository defRepo,
        ILogger<PacketStreamParser> logger)
    {
        _contentParser = contentParser;
        _defRepo = defRepo;
        _logger = logger;
    }

    public async IAsyncEnumerable<TelemetryPacketWithUIntTime> Parse(
        Stream stream,
        Dictionary<ushort, IReadOnlyCollection<SensorId>> sensors,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var env = new ParserEnvironment(stream, sensors, _contentParser, _defRepo, _logger);
        var stateData = new PacketState();
        IParserState state = new SyncState();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = await state.HandleAsync(env, stateData, cancellationToken);
            if (state is ParsedState ps)
                yield return ps.CreatePacket(stateData);
            if (next == null)
                yield break;
            state = next;
        }
    }
}