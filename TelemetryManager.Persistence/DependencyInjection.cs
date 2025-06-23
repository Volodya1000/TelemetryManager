using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Persistence.Repositories;

namespace TelemetryManager.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistance(this IServiceCollection services)
    {
        services.AddDbContext<TelemetryContext>(options =>
            options.UseSqlite("Data Source=TelemetryManagerSqliteDataBase.db;"));
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IContentDefinitionRepository, ContentDefinitionRepository>();

        return services;
    }
}
