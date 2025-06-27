using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.AvaloniaApp.ViewModels.DialogInteractionParams;

namespace TelemetryManager.AvaloniaApp.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow(MainWindowViewModel viewModel, 
                     Func<DeviceSensorsParams,DeviceSensorsWindow> dialogFactory)
    {
        InitializeComponent();
        ViewModel = viewModel;

        this.WhenActivated(disposables =>
        {
            ViewModel.ShowDeviceSensorsDialogInteraction.RegisterHandler(async interaction =>
            {
                var dialog = dialogFactory(interaction.Input);

                var res = await dialog.ShowDialog<Unit>(this);

                interaction.SetOutput(res);
            }).DisposeWith(disposables);

            //ViewModel.OpenDeviceSensorsWindowCommand
            //    .Subscribe(async _ => await dialogFactory().ShowDialog(this))
            //    .DisposeWith(disposables);
        });

    }
}