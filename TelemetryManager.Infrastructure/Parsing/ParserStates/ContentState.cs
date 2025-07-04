using Microsoft.Extensions.Logging;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class ContentState : IParserState
{
    public async Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        try
        {
            state.Content = new byte[state.Size];
            int read = 0;
            while (read < state.Size)
            {
                int r = await env.Stream.ReadAsync(state.Content, read, state.Size - read, ct);
                if (r == 0) throw new EndOfStreamException();
                read += r;
            }
            state.Padding = PacketHelper.CalculatePadding(state.Size);
            if (state.Padding > 0)
                env.Stream.Position += state.Padding;
            return new ChecksumState();
        }
        catch (Exception ex)
        {
            env.Logger.LogError(ex, "Content read error at {Offset}", state.PacketStart);
            return new SyncState();
        }
    }
}
