using TelemetryManager.Infrastructure.Parsing.Data;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class SyncState : IParserState
{
    public async Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        state.PacketStart = await FindSyncAsync(env.Stream, ct);
        if (state.PacketStart < 0) return null;
        env.Stream.Position = state.PacketStart + PacketConstants.SyncMarkerLength;
        return new HeaderState();
    }

    private async Task<long> FindSyncAsync(Stream s, CancellationToken ct)
    {
        var marker = PacketConstants.SyncMarkerBytes;
        int first = marker[0];
        byte[] buffer = new byte[1];
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            int read = await s.ReadAsync(buffer, 0, 1, ct);
            if (read == 0) return -1;
            if (buffer[0] != first) continue;
            long pos = s.Position - 1;
            byte[] peek = new byte[marker.Length];
            peek[0] = (byte)first;
            int got = await s.ReadAsync(peek, 1, marker.Length - 1, ct);
            if (got < marker.Length - 1)
            {
                s.Position = pos + 1;
                continue;
            }
            if (peek.AsSpan().SequenceEqual(marker)) return pos;
            s.Position = pos + 1;
        }
    }
}
