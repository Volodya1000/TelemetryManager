using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.ContentTypeProcessing;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;

namespace TelemetryManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IContentTypeParser, ContentTypeParser>();

        serviceCollection.AddScoped<TelemetryProcessingService>()
            .AddScoped<ParameterValidationService>()
            .AddScoped<DeviceService>()
            .AddScoped<IContentDefinitionService, ContentDefinitionService>()
            .AddScoped<SensorContentRegistrar>()
            .AddSingleton<IDataTypeHandlerService, DataTypeHandlerService>();

        return serviceCollection;
    }
}