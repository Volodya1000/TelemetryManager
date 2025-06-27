using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Extensions;
using TelemetryManager.AvaloniaApp.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
using TelemetryManager.ViewModels.ViewModelsFolder;
using TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

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
            services.AddAllServices();

            // Создаем главное окно для получения StorageProvider
            var tempProvider = services.BuildServiceProvider();
            var mainWindow = tempProvider.GetRequiredService<MainWindow>();

            // Получаем StorageProvider
            var storageProvider = TopLevel.GetTopLevel(mainWindow)?.StorageProvider
                ?? throw new InvalidOperationException("Cannot get StorageProvider");

            // Добавляем сервисы, требующие StorageProvider
            services.AddSingleton(storageProvider);
            services.AddSingleton<IFileSelectionService, AvaloniaFileSelectionService>();

            // Строим финальный провайдер сервисов
            ServiceProvider = services.BuildServiceProvider();

            // Регистрируем устройства и сенсоры
            var contentRegistrar = ServiceProvider.GetRequiredService<DeviceContentRegistrar>();
            contentRegistrar.RegisterAllContent();

            // Генерируем телеметрию
            GenerateTelemetryData();

            // Устанавливаем главное окно и DataContext
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
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
}