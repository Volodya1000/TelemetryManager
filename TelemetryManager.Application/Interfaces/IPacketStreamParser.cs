using TelemetryManager.Core.Data;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Interfaces;

public interface IPacketStreamParser
{
    public PacketParsingResult Parse(Stream stream, Dictionary<ushort, IReadOnlyCollection<SensorId>> availableSensorsInDevices);
}
