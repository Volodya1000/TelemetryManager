namespace TelemetryManager.Core.Data;

public class PacketParsingResult
{
    public IReadOnlyList<TelemetryPacket> Packets { get; init; }
    public IReadOnlyList<ParsingError> Errors { get; init; }

    public PacketParsingResult(List<TelemetryPacket> packets, List<ParsingError> errors)
    {
        Packets = packets;
        Errors = errors;
    }
}