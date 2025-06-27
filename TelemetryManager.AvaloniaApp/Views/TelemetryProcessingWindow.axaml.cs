
using Avalonia.ReactiveUI;
using TelemetryManager.AvaloniaApp.ViewModels;
namespace TelemetryManager.AvaloniaApp.Views;

public partial class TelemetryProcessingWindow : ReactiveWindow<TelemetryProcessingViewModel>
{
    public TelemetryProcessingWindow(TelemetryProcessingViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }
}