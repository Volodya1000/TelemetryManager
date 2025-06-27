using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using TelemetryManager.Application;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.ViewModels.DialogInteractionParams;
using TelemetryManager.AvaloniaApp.ViewModels.ViewModelsServicesInterfaces;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure;
using TelemetryManager.Persistence;

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
       // .AddAvaloniaServices();

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

    //public static IServiceCollection AddAvaloniaServices(this IServiceCollection serviceCollection)
    //{
    //    serviceCollection.AddSingleton<IFileSelectionService>(provider =>
    //    {
    //        var topLevel = provider.GetRequiredService<TopLevel>();
    //        return new AvaloniaFileSelectionService(topLevel);
    //    });

    //    return serviceCollection;
    //}

    //public static IServiceCollection AddAvaloniaServices(this IServiceCollection serviceCollection)
    //{
    //    // Регистрируем фабрику для IFileSelectionService
    //    serviceCollection.AddSingleton<IFileSelectionService>(provider =>
    //    {
    //        // Получаем IClassicDesktopStyleApplicationLifetime
    //        var lifetime = provider.GetRequiredService<IClassicDesktopStyleApplicationLifetime>();

    //        // Получаем TopLevel из главного окна
    //        var topLevel = TopLevel.GetTopLevel(lifetime.MainWindow);
    //        if (topLevel == null)
    //        {
    //            throw new InvalidOperationException("Unable to get TopLevel from MainWindow");
    //        }

    //        return new AvaloniaFileSelectionService(topLevel);
    //    });

    //    return serviceCollection;
    //}

    public static IServiceCollection AddAvaloniaServices(
          this IServiceCollection services,
          IStorageProvider storageProvider)
    {
        services.AddSingleton(storageProvider);
        services.AddSingleton<IFileSelectionService, AvaloniaFileSelectionService>();
        return services;
    }
}