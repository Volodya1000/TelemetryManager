using Microsoft.Extensions.Logging;
using System.Buffers.Binary;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class HeaderState : IParserState
{
    public async Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        try
        {
            byte[] header = new byte[PacketHelper.HeaderLength];
            int read = await env.Stream.ReadAsync(header, 0, header.Length, ct);
            if (read != header.Length)
                throw new InvalidDataException("Incomplete header");

            state.Header = header;
            state.Timestamp = BinaryPrimitives.ReadUInt32BigEndian(header);
            state.DeviceId = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(4));
            state.TypeId = header[6];
            state.SourceId = header[7];
            state.Size = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(8));
            return new ValidationState();
        }
        catch (Exception ex)
        {
            env.Logger.LogError(ex, "Header parse error at {Offset}", state.PacketStart);
            return new SyncState();
        }
    }
}
