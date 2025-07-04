using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Interfaces;

public interface IPacketStreamParser
{
    public IAsyncEnumerable<TelemetryPacketWithUIntTime> Parse(
       Stream stream,
       Dictionary<ushort, IReadOnlyCollection<SensorId>> sensors,
       CancellationToken cancellationToken = default);
}
