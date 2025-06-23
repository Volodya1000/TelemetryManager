using System.Runtime.CompilerServices;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Interfaces;

public interface IPacketStreamParser
{
    public  IAsyncEnumerable<TelemetryPacketWithUIntTime> Parse(
      Stream stream,
      Dictionary<ushort, IReadOnlyCollection<SensorId>> availableSensorsInDevices,
      Action<ParsingError> errorCallback = null,
      [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
