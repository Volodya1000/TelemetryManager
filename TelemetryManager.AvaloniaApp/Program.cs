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

        // Устройство 1 - Метеостанция
        deviceService.AddAsync(1, "WeatherStation").Wait();
        // Температура
        deviceService.AddSensorWithParametersAsync(1, 1, 1, "OutdoorTempSensor").Wait();
        deviceService.SetParameterIntervalAsync(1, 1, 1, "TemperatureInFahrenheit", -40, 120).Wait();
        // Давление
        deviceService.AddSensorWithParametersAsync(1, 2, 1, "BarometricSensor").Wait();
        deviceService.SetParameterIntervalAsync(1, 2, 1, "Pressure", 80, 110).Wait();
        // Влажность
        deviceService.AddSensorWithParametersAsync(1, 7, 1, "HumiditySensor").Wait();
        deviceService.SetParameterIntervalAsync(1, 7, 1, "Humidity", 0, 100).Wait();

        // Устройство 2 - Навигационный модуль
        deviceService.AddAsync(2, "NavigationModule").Wait();
        // Акселерометр
        deviceService.AddSensorWithParametersAsync(2, 3, 1, "MainAccelerometer").Wait();
        deviceService.SetParameterIntervalAsync(2, 3, 1, "X", -16, 16).Wait();
        deviceService.SetParameterIntervalAsync(2, 3, 1, "Y", -16, 16).Wait();
        deviceService.SetParameterIntervalAsync(2, 3, 1, "Z", -16, 16).Wait();
        // Магнетометр
        deviceService.AddSensorWithParametersAsync(2, 4, 1, "CompassSensor").Wait();
        deviceService.SetParameterIntervalAsync(2, 4, 1, "X", -100, 100).Wait();
        deviceService.SetParameterIntervalAsync(2, 4, 1, "Y", -100, 100).Wait();
        deviceService.SetParameterIntervalAsync(2, 4, 1, "Z", -100, 100).Wait();
        // Гироскоп
        deviceService.AddSensorWithParametersAsync(2, 6, 1, "GyroUnit").Wait();
        deviceService.SetParameterIntervalAsync(2, 6, 1, "X", -20, 20).Wait();
        deviceService.SetParameterIntervalAsync(2, 6, 1, "Y", -20, 20).Wait();
        deviceService.SetParameterIntervalAsync(2, 6, 1, "Z", -20, 20).Wait();

        // Устройство 3 - Промышленный монитор
        deviceService.AddAsync(3, "IndustrialMonitor").Wait();
        // Датчик падения
        deviceService.AddSensorWithParametersAsync(3, 5, 1, "FallDetectionSensor").Wait();
        deviceService.SetParameterIntervalAsync(3, 5, 1, "AccelerationThreshold", 0, 2).Wait();
        // Температура (в Цельсиях)
        deviceService.AddSensorWithParametersAsync(3, 1, 1, "InternalTempSensor").Wait();
        deviceService.SetParameterIntervalAsync(3, 1, 1, "TemperatureInFahrenheit", -20, 60).Wait();
        // Давление
        deviceService.AddSensorWithParametersAsync(3, 2, 2, "VentilationPressureSensor").Wait();
        deviceService.SetParameterIntervalAsync(3, 2, 2, "Pressure", 90, 105).Wait();






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
