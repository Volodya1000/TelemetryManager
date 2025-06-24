using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
using TelemetryManager.Application;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Extensions;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Infrastructure;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
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

        var registrar = ServiceProvider.GetRequiredService<SensorContentRegistrar>();
        registrar.RegisterAllSensorTypesAsync().Wait();

        var deviceService = ServiceProvider.GetRequiredService<DeviceService>();
         deviceService.AddAsync(1, "MyFirstDevice").Wait();
         deviceService.AddSensorWithParametersAsync(1, 1, 1, "MyFirstTemperatureSensor").Wait();
         deviceService.SetParameterIntervalAsync(1, 1, 1, "TemperatureInFahrenheit", -30, 30).Wait();






        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

        string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");
        var generator = ServiceProvider.GetRequiredService<TelemetryGenerator>();

        generator.Generate(telemetryFilePath, 50, 0).Wait();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
