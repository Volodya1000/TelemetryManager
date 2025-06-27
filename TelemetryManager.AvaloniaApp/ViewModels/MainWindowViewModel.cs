using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Models;
using TelemetryManager.AvaloniaApp.ViewModels.DialogInteractionParams;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private readonly IContentDefinitionRepository _contentDefinitionRepository;

    [Reactive] public Window? OwnerWindow { get; set; }
    [Reactive] public string NewDeviceName { get; set; } = "";
    [Reactive] public ushort NewDeviceId { get; set; }
    [Reactive] public string ErrorMessage { get; set; }
    [Reactive] public DeviceDisplayItem SelectedDevice { get; set; }

    public ObservableCollection<DeviceDisplayItem> Devices { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadDevicesCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDeviceCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenDeviceSensorsWindowCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenTelemetryProcessingCommand { get; }

    public Interaction<DeviceSensorsParams, Unit> ShowDeviceSensorsDialogInteraction { get; } = new();

    public MainWindowViewModel(DeviceService deviceService, IContentDefinitionRepository contentDefinitionRepository)
    {
        _deviceService = deviceService;
        _contentDefinitionRepository = contentDefinitionRepository;

        LoadDevicesCommand = ReactiveCommand.CreateFromTask(LoadDevicesAsync);
        OpenTelemetryProcessingCommand = ReactiveCommand.CreateFromTask(OpenTelemetryProcessingAsync);

        // Обработка ошибок загрузки
        LoadDevicesCommand.ThrownExceptions.Subscribe(ex =>
            ErrorMessage = $"Ошибка загрузки: {ex.Message}");

        var canAddDevice = this.WhenAnyValue(
            x => x.NewDeviceName,
            x => x.NewDeviceId,
            (name, id) => !string.IsNullOrWhiteSpace(name) && id > 0
        );

        AddDeviceCommand = ReactiveCommand.CreateFromTask(AddDeviceAsync, canAddDevice);

        OpenDeviceSensorsWindowCommand = ReactiveCommand.CreateFromTask(
            OpenDialogAsync,
            this.WhenAnyValue(x => x.SelectedDevice).Select(device => device != null));

        // Обработка ошибок добавления
        AddDeviceCommand.ThrownExceptions.Subscribe(ex =>
            ErrorMessage = $"Ошибка добавления: {ex.Message}");
      

        LoadDevicesCommand.Execute().Subscribe();
    }


    private async Task OpenDialogAsync()
    {
        var param = new DeviceSensorsParams { DeviceId= SelectedDevice.DeviceId};
        var result = await ShowDeviceSensorsDialogInteraction.Handle(param);
    }


    private async Task LoadDevicesAsync()
    {
        ErrorMessage = ""; // Сброс ошибки при загрузке
        Devices.Clear();
        var devices = await _deviceService.GetAllAsync();
        foreach (var device in devices)
        {
            Devices.Add(new DeviceDisplayItem
            {
                DeviceId = device.DeviceId,
                Name = device.Name.Value,
                ActivationTime = device.ActivationTime?.ToString("dd.MM.yyyy HH:mm") ?? "Не активирован"
            });
        }
    }

    private async Task AddDeviceAsync()
    {
        ErrorMessage = ""; // Сброс ошибки перед операцией
        await _deviceService.AddAsync(NewDeviceId, NewDeviceName);
        NewDeviceName = "";
        NewDeviceId = 0;
        await LoadDevicesAsync();
    }

    private async Task OpenTelemetryProcessingAsync()
    {
        var window = new TelemetryProcessingWindow();
        await window.ShowDialog(OwnerWindow);
    }
}