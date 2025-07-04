using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Buffers;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class ChecksumState : IParserState
{
    public async Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        byte[] csBuf = ArrayPool<byte>.Shared.Rent(2);
        try
        {
            int got = await env.Stream.ReadAsync(csBuf, 0, 2, ct);
            if (got != 2) throw new InvalidDataException("Checksum read failed");

            ushort expected = BinaryPrimitives.ReadUInt16BigEndian(csBuf);
            var combined = PacketHelper.CombineArrays(state.Header, state.Content, new byte[state.Padding]);
            ushort actual = ChecksumCalculator.Compute(combined);
            if (expected != actual)
                throw new InvalidDataException("Checksum mismatch");

            var values = await env.ContentParser.ParseAsync(state.TypeId, state.Content);
            env.Logger.LogInformation("Parsed packet at {Offset} successfully", state.PacketStart);

            return new ParsedState(values);
        }
        catch (Exception ex)
        {
            env.Logger.LogError(ex, "Checksum or parse error at {Offset}", state.PacketStart);
            return new SyncState();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(csBuf);
        }
    }
}