using Avalonia.ReactiveUI;
using TelemetryManager.AvaloniaApp.ViewModels;
namespace TelemetryManager.AvaloniaApp.Views;

public partial class DeviceSensorsWindow : ReactiveWindow<DeviceSensorsViewModel>
{
    public DeviceSensorsWindow(DeviceSensorsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel=viewModel;
    }
}