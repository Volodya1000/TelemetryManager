using TelemetryManager.Core.Data.TelemetryPackets;

namespace TelemetryManager.Core.Data;

public class PacketParsingResult
{
    public IReadOnlyCollection<TelemetryPacketWithUIntTime> Packets { get; init; }
    public IReadOnlyCollection<ParsingError> Errors { get; init; }

    public PacketParsingResult(List<TelemetryPacketWithUIntTime> packets, List<ParsingError> errors)
    {
        Packets = packets;
        Errors = errors;
    }
}