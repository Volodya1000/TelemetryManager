using TelemetryManager.Core.Data;

namespace TelemetryManager.Application.Interfaces;

public interface IPacketStreamParser
{
    PacketParsingResult Parse(Stream stream);
   // IReadOnlyList<ParsingError> Errors { get; }
}
