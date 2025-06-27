using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
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
using TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

namespace TelemetryManager.AvaloniaApp.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPersistance()
        .AddApplication()
        .AddInfrastructure()
        .AddApplicationWindows()
        .AddApplicationViewModels();
        serviceCollection.AddTransient<DeviceContentRegistrar>();

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationWindows(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindow>();
        serviceCollection.AddTransient<TelemetryProcessingWindow>();
        serviceCollection.AddTransient<Func<DeviceSensorsParams, DeviceSensorsWindow>>(provider =>
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
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddTransient<TelemetryProcessingViewModel>();

        return serviceCollection;
    }

    public static IServiceCollection AddAvaloniaServices(
          this IServiceCollection services,
          IStorageProvider storageProvider)
    {
        services.AddSingleton(storageProvider);
        services.AddSingleton<IFileSelectionService, AvaloniaFileSelectionService>();

        return services;
    }
}