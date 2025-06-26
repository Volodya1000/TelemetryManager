using Avalonia.ReactiveUI;
using TelemetryManager.AvaloniaApp.ViewModels;

namespace TelemetryManager.AvaloniaApp.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        if (DataContext is MainWindowViewModel vm)
        {
            vm.OwnerWindow = this;
        }
    }
}