namespace TelemetryManager.Infrastructure.Parsing;

interface IParserState
{
    Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct);
}
