using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.Views;
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
        serviceCollection.AddTransient<DeviceSensorsWindow>();
        serviceCollection.AddTransient<TelemetryProcessingWindow>();

        return serviceCollection;
    }

    public static IServiceCollection AddApplicationWindows(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddTransient<DeviceSensorsViewModel>();
        serviceCollection.AddTransient<TelemetryProcessingViewModel>();

        return serviceCollection;
    }
}