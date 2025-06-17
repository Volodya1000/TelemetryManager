using TelemetryManager.Core.Data;

namespace TelemetryManager.Application.Interfaces;

public interface IPacketStreamParser
{
    public List<TelemetryPacket> Parse(Stream stream);
   // IReadOnlyList<ParsingError> Errors { get; }
}
