using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application;
using TelemetryManager.Infrastructure;
using TelemetryManager.Persistence;

namespace TelemetryManager.Avalonia.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPersistance();
        serviceCollection.AddApplication();
        serviceCollection.AddInfrastructure();

        return serviceCollection;
    }
}
