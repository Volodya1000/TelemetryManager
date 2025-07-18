﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using TelemetryManager.Application.Services;
using TelemetryManager.ViewModels.DialogInteractionParams;
using TelemetryManager.ViewModels.ModelsForUI;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class MainWindowViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;

    [Reactive] public string NewDeviceName { get; set; } = "";
    [Reactive] public ushort NewDeviceId { get; set; }
    [Reactive] public string ErrorMessage { get; set; }
    [Reactive] public DeviceDisplayItem SelectedDevice { get; set; }

    public ObservableCollection<DeviceDisplayItem> Devices { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadDevicesCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDeviceCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenDeviceSensorsWindowCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenTelemetryProcessingCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCreateTelemetryWindowCommand { get; }

    public Interaction<DeviceSensorsParams, Unit> ShowDeviceSensorsDialogInteraction { get; } = new();
    public Interaction<Unit, Unit> ShowTelemetryProcessingInteraction { get; } = new();

    public MainWindowViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;

        LoadDevicesCommand = ReactiveCommand.CreateFromTask(LoadDevicesAsync);

        // Обработка ошибок загрузки
        LoadDevicesCommand.ThrownExceptions.Subscribe(ex =>
            ErrorMessage = $"Ошибка загрузки: {ex.Message}");

        var canAddDevice = this.WhenAnyValue(
            x => x.NewDeviceName,
            x => x.NewDeviceId,
            (name, id) => !string.IsNullOrWhiteSpace(name) && id > 0
        );

        AddDeviceCommand = ReactiveCommand.CreateFromTask(AddDeviceAsync, canAddDevice);

        OpenTelemetryProcessingCommand = ReactiveCommand.CreateFromTask(OpenTelemetryProcessingAsync);

        OpenDeviceSensorsWindowCommand = ReactiveCommand.CreateFromTask(
            OpenDeviceSensorsDialogAsync,
            this.WhenAnyValue(x => x.SelectedDevice).Select(device => device != null));

        // Обработка ошибок добавления
        AddDeviceCommand.ThrownExceptions.Subscribe(ex =>
            ErrorMessage = $"Ошибка добавления: {ex.Message}");

        OpenCreateTelemetryWindowCommand = ReactiveCommand.CreateFromTask(()=>Task.CompletedTask);


        LoadDevicesCommand.Execute().Subscribe();
    }

    private async Task LoadDevicesAsync()
    {
        ErrorMessage = ""; // Сброс ошибки при загрузке
        Devices.Clear();
        var devices = await _deviceService.GetAllAsync();
        foreach (var device in devices)
        {
            Devices.Add(new DeviceDisplayItem(
             DeviceId: device.DeviceId,
             Name: device.Name,
             ActivationTime: device.ActivationTime?.ToString("dd.MM.yyyy HH:mm") ?? "Не активирован"
         ));
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


    #region Open dialogs

    private async Task OpenTelemetryProcessingAsync()
    {
        await ShowTelemetryProcessingInteraction.Handle(Unit.Default);

        await LoadDevicesAsync();
    }

    private async Task OpenDeviceSensorsDialogAsync()
    {
        var param = new DeviceSensorsParams { DeviceId = SelectedDevice.DeviceId };
        var result = await ShowDeviceSensorsDialogInteraction.Handle(param);
    }
    #endregion
}
