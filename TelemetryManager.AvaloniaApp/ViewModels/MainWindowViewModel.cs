using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.Models;
using TelemetryManager.AvaloniaApp.Views;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private readonly IContentDefinitionRepository _contentDefinitionRepository;
    private string _newDeviceName = "";
    private ushort _newDeviceId;
    private string _errorMessage; // Новое свойство для ошибки

    public Window? OwnerWindow { get; set; }

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

    // Свойство для сообщения об ошибке
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> LoadDevicesCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDeviceCommand { get; }



    private DeviceDisplayItem _selectedDevice;
    public DeviceDisplayItem SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    public ReactiveCommand<Unit, Unit> ManageSensorsCommand { get; }



    public MainWindowViewModel(DeviceService deviceService, IContentDefinitionRepository  contentDefinitionRepository)
    {
        _deviceService = deviceService;
        _contentDefinitionRepository=contentDefinitionRepository;

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

        ManageSensorsCommand = ReactiveCommand.CreateFromTask(ManageSensorsAsync,
      this.WhenAnyValue(x => x.SelectedDevice).Select(device => device != null));

        // Обработка ошибок добавления
        AddDeviceCommand.ThrownExceptions.Subscribe(ex =>
            ErrorMessage = $"Ошибка добавления: {ex.Message}");

        LoadDevicesCommand.Execute().Subscribe();
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

    private async Task ManageSensorsAsync()
    {
        if (SelectedDevice == null) return;

        var vm = new DeviceSensorsViewModel(
            SelectedDevice.DeviceId,
            _deviceService,
            _contentDefinitionRepository
        );

        var window = new DeviceSensorsWindow { DataContext = vm };

        // Получаем ссылку на главное окно через TopLevel
        var mainWindow = (Window)TopLevel.GetTopLevel((Visual)this.OwnerWindow);
        await window.ShowDialog(mainWindow);
    }
}