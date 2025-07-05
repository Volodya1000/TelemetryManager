using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using TelemetryManager.ViewModels.ViewModelsFolder;

namespace TelemetryManager.AvaloniaApp.Views;

public partial class CreateTelemetryWindow : ReactiveWindow<CreateTelemetryViewModel>
{
    public CreateTelemetryWindow(CreateTelemetryViewModel viewModel)
    {
        InitializeComponent();

        ViewModel=viewModel;
    }
}