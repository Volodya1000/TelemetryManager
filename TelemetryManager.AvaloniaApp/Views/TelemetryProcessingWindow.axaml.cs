using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.ViewModels;
namespace TelemetryManager.AvaloniaApp.Views;

public partial class TelemetryProcessingWindow : Window
{
    public TelemetryProcessingWindow()
    {
        InitializeComponent();

        DataContext = new TelemetryProcessingViewModel(Program.ServiceProvider
                        .GetRequiredService<TelemetryProcessingService>(), TopLevel.GetTopLevel(this));
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}