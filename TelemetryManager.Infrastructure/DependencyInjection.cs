using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Infrastructure.FileReader;
using TelemetryManager.Infrastructure.Parsing;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

namespace TelemetryManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IPacketStreamParser, PacketStreamParser>();
        serviceCollection.AddScoped<IFileReaderService, FileReaderService>();
        serviceCollection.AddScoped<FileReaderService>();
        serviceCollection.AddScoped<TelemetryGenerator>();
        serviceCollection.AddScoped<ContentGenerator>();

        return serviceCollection;
    }
}
