using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Repositories;

namespace TelemetryManager.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistance(this IServiceCollection services)
    {
        services.AddTransient<ITelemetryRepository, TelemetryRepository>();
        services.AddTransient<IDeviceRepository, DeviceRepository>();
        services.AddTransient<IContentDefinitionRepository, ContentDefinitionRepository>();
        return services;
    }
}
