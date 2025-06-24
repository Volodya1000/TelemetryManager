using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using System;
using TelemetryManager.Application;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.AvaloniaApp.Extensions;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Infrastructure;
using TelemetryManager.Persistence;

namespace TelemetryManager.AvaloniaApp;

internal sealed class Program
{
    public static IServiceProvider ServiceProvider { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAllServices();
        services.AddTransient<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        ServiceProvider = services.BuildServiceProvider();


        var contentDefinitionService = ServiceProvider.GetRequiredService<IContentDefinitionService>();

         contentDefinitionService.RegisterAsync(
            id: 1,
            contentName: "FarengaitTemperatureSensorContent",
            parameters: new[]
            {
        ("TemperatureInFarengeit", "Temperature", "Farengait", typeof(float))
            }
        ).Wait();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
