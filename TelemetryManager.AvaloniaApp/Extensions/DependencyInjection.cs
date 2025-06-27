using Microsoft.Extensions.DependencyInjection;
using System;
using TelemetryManager.Application;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.ViewModels.DialogInteractionParams;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure;
using TelemetryManager.Persistence;

namespace TelemetryManager.AvaloniaApp.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPersistance();
        serviceCollection.AddApplication();
        serviceCollection.AddInfrastructure();

        AddApplicationWindows(serviceCollection);
        AddApplicationViewModels(serviceCollection);

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationWindows(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindow>();
        serviceCollection.AddTransient<TelemetryProcessingWindow>();

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationViewModels(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddTransient<TelemetryProcessingViewModel>();
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
}