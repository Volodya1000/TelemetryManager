using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using TelemetryManager.ViewModels.DialogInteractionParams;
using TelemetryManager.ViewModels.ViewModelsFolder;

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

            ViewModel.ShowTelemetryProcessingInteraction
            .RegisterHandler(async interaction =>
            {
                var window = App.ServiceProvider.GetRequiredService<TelemetryProcessingWindow>();
                await window.ShowDialog<Unit>(this);
                interaction.SetOutput(Unit.Default);
            })
            .DisposeWith(disposables);

            ViewModel.OpenCreateTelemetryWindowCommand
           .Subscribe(_ =>
           {
               var window = App.ServiceProvider.GetRequiredService<CreateTelemetryWindow>();
               window.Show();
           })
           .DisposeWith(disposables);
        });

    }
}