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

        serviceCollection.AddScoped<TelemetryProcessingService>();
        serviceCollection.AddScoped<ParameterValidationService>();
        serviceCollection.AddScoped<DeviceService>();
        serviceCollection.AddScoped<IContentDefinitionService, ContentDefinitionService>();

        return serviceCollection;
    }
}