using TelemetryManager.Core.Data;

namespace TelemetryManager.Core.Interfaces.Repositories;

public interface ITelemetryRepository
{
    Task AddPacketAsync(TelemetryPacketWithDate packet);
    Task<PagedResponse<TelemetryPacketWithDate>> GetPacketsAsync(TelemetryPacketRequestFilter filter);
}
