using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using TelemetryManager.Application;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Services;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure;
using TelemetryManager.Persistence;
using TelemetryManager.ViewModels.DialogInteractionParams;
using TelemetryManager.ViewModels.ViewModelsFolder;

namespace TelemetryManager.AvaloniaApp.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
        .AddLoggingWithSerilog()
        .AddPersistance()
        .AddApplication()
        .AddInfrastructure()
        .AddApplicationWindows()
        .AddApplicationViewModels();
        serviceCollection.AddTransient<DeviceContentRegistrar>();       

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationWindows(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindow>()
            .AddTransient<TelemetryProcessingWindow>()
            .AddTransient<Func<DeviceSensorsParams, DeviceSensorsWindow>>(provider =>
             param =>
             {
                 var deviceService = provider.GetRequiredService<DeviceService>();
                 var contentRepo = provider.GetRequiredService<IContentDefinitionRepository>();
                 var viewModel = new DeviceSensorsViewModel(
                     param.DeviceId,
                     deviceService,
                     contentRepo);
                 return new DeviceSensorsWindow(viewModel);
             });
        return serviceCollection;
    }

    public static IServiceCollection AddApplicationViewModels(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindowViewModel>()
            .AddTransient<TelemetryProcessingViewModel>();

        return serviceCollection;
    }

    public static IServiceCollection AddLoggingWithSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/telemetry.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });

        return services;
    }
}