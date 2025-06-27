using Avalonia.ReactiveUI;
using TelemetryManager.ViewModels.ViewModelsFolder;

namespace TelemetryManager.AvaloniaApp.Views;

public partial class ChangeIntervalWindow : ReactiveWindow<ChangeIntervalViewModel>
{
    public ChangeIntervalWindow(ChangeIntervalViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }
}