using Avalonia.ReactiveUI;
using TelemetryManager.ViewModels.ViewModelsFolder;
namespace TelemetryManager.AvaloniaApp.Views;

public partial class DeviceSensorsWindow : ReactiveWindow<DeviceSensorsViewModel>
{
    public DeviceSensorsWindow(DeviceSensorsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel=viewModel;
    }
}