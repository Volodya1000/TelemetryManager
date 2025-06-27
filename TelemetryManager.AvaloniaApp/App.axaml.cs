using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using TelemetryManager.AvaloniaApp.Extensions;
using TelemetryManager.AvaloniaApp.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.ViewModels.ViewModelsServicesInterfaces;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

namespace TelemetryManager.AvaloniaApp;

public partial class App : Avalonia.Application
{
    public  static IServiceProvider ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Создаем и конфигурируем сервисы
            var services = new ServiceCollection();
            ConfigureCoreServices(services);

            // Строим провайдер сервисов ДО создания окон
            ServiceProvider = services.BuildServiceProvider();

            // Создаем главное окно через DI
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;

            // Добавляем Avalonia-специфичные сервисы
            var storageProvider = TopLevel.GetTopLevel(mainWindow)?.StorageProvider
                ?? throw new InvalidOperationException("Cannot get StorageProvider");

            // Перестраиваем ServiceProvider с новыми сервисами
            services.AddSingleton(storageProvider);
            services.AddSingleton<IFileSelectionService, AvaloniaFileSelectionService>();
            ServiceProvider = services.BuildServiceProvider();

            // Регистрируем устройства и сенсоры
            var contentRegistrar = ServiceProvider.GetRequiredService<DeviceContentRegistrar>();
            contentRegistrar.RegisterAllContent();

            // Генерируем телеметрию
            GenerateTelemetryData();

            // Устанавливаем DataContext
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void GenerateTelemetryData()
    {
        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
        string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");

        var generator = ServiceProvider.GetRequiredService<TelemetryGenerator>();
        generator.Generate(telemetryFilePath, 50, 0).Wait();
    }

    private void ConfigureCoreServices(IServiceCollection services)
    {
        services.AddAllServices();
        services.AddTransient<DeviceContentRegistrar>();
    }

    private void ConfigureAvaloniaServices(IServiceCollection services, IStorageProvider storageProvider)
    {
        services.AddSingleton(storageProvider);
        services.AddSingleton<IFileSelectionService, AvaloniaFileSelectionService>();
    }
}