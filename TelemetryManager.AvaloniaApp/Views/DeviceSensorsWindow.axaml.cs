using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TelemetryManager.AvaloniaApp.Views;

public partial class DeviceSensorsWindow : Window
{
    public DeviceSensorsWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}