using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
using TelemetryManager.Application;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Extensions;
using TelemetryManager.AvaloniaApp.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Infrastructure;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
using TelemetryManager.Persistence;

namespace TelemetryManager.AvaloniaApp;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}