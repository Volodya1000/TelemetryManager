using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.ContentTypeProcessing;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure.FileReader;
using TelemetryManager.Infrastructure.Parsing;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
using TelemetryManager.Persistence;
using TelemetryManager.Persistence.InMemoryRepositories;

namespace TelemetryManager.Console;

public static class DependencyInjection
{
    public static IServiceCollection AddConsole(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IPacketStreamParser, PacketStreamParser>();
        serviceCollection.AddTransient<IContentTypeParser, ContentTypeParser>();

        serviceCollection.AddPersistance();
        serviceCollection.AddScoped<TelemetryProcessingService>();
        serviceCollection.AddScoped<ParameterValidationService>();
        serviceCollection.AddScoped<DeviceService>();
        serviceCollection.AddScoped<IContentDefinitionService, ContentDefinitionService>();
        serviceCollection.AddScoped<IFileReaderService, FileReaderService>();

        serviceCollection.AddScoped<FileReaderService>();

        serviceCollection.AddScoped<TelemetryGenerator>();
        serviceCollection.AddScoped<ContentGenerator>();

        return serviceCollection;
    }
}

