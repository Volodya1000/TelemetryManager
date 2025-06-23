using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class DeviceDisplayItem
{
    public ushort DeviceId { get; set; }
    public string Name { get; set; }
    public string ActivationTime { get; set; }
}

public class MainWindowViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private string _newDeviceName = "";
    private ushort _newDeviceId;

    public ObservableCollection<DeviceDisplayItem> Devices { get; } = new();
    public string NewDeviceName
    {
        get => _newDeviceName;
        set => this.RaiseAndSetIfChanged(ref _newDeviceName, value);
    }

    public ushort NewDeviceId
    {
        get => _newDeviceId;
        set => this.RaiseAndSetIfChanged(ref _newDeviceId, value);
    }

    public ReactiveCommand<Unit, Unit> LoadDevicesCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDeviceCommand { get; }

    public MainWindowViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;

        LoadDevicesCommand = ReactiveCommand.CreateFromTask(LoadDevicesAsync);

        var canAddDevice = this.WhenAnyValue(
            x => x.NewDeviceName,
            x => x.NewDeviceId,
            (name, id) => !string.IsNullOrWhiteSpace(name) && id > 0
        );

        AddDeviceCommand = ReactiveCommand.CreateFromTask(AddDeviceAsync, canAddDevice);

        LoadDevicesCommand.Execute().Subscribe();
    }

    private async Task LoadDevicesAsync()
    {
        Devices.Clear();
        var devices = await _deviceService.GetAllAsync();
        foreach (var device in devices)
        {
            Devices.Add(new DeviceDisplayItem
            {
                DeviceId = device.DeviceId,
                Name = device.Name.ToString(),
                ActivationTime = device.ActivationTime?.ToString("dd.MM.yyyy HH:mm") ?? "Не активирован"
            });
        }
    }

    private async Task AddDeviceAsync()
    {
        await _deviceService.AddAsync(NewDeviceId, NewDeviceName);
        NewDeviceName = "";
        NewDeviceId = 0;
        await LoadDevicesAsync();
    }
}
