using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class ParsedState : IParserState
{
    private readonly IReadOnlyDictionary<string, double> _values;

    public ParsedState(IReadOnlyDictionary<string, double> values)
    {
        _values = values;
    }

    public Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        return Task.FromResult<IParserState>(new SyncState());
    }

    public TelemetryPacketWithUIntTime CreatePacket(PacketState state)
        => new TelemetryPacketWithUIntTime(
            state.Timestamp,
            state.DeviceId,
            new SensorId(state.TypeId, state.SourceId),
            _values);
}
