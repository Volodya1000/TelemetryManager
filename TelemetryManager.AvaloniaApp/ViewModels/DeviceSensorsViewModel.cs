using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class DeviceSensorsViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private readonly IContentDefinitionRepository _contentRepo;
    private ushort _deviceId;

    public ObservableCollection<SensorItemViewModel> Sensors { get; } = new();
    public ObservableCollection<ContentDefinition> AvailableSensorTypes { get; } = new();

    private ContentDefinition _selectedSensorType;
    public ContentDefinition SelectedSensorType
    {
        get => _selectedSensorType;
        set => this.RaiseAndSetIfChanged(ref _selectedSensorType, value);
    }

    private byte _sourceId;
    public byte SourceId
    {
        get => _sourceId;
        set => this.RaiseAndSetIfChanged(ref _sourceId, value);
    }

    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public string DeviceHeader => $"Управление сенсорами устройства ID: {_deviceId}";
    public ReactiveCommand<Unit, Unit> LoadSensorsCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSensorCommand { get; }

    public DeviceSensorsViewModel(
        ushort deviceId,
        DeviceService deviceService,
        IContentDefinitionRepository contentRepo)
    {
        _deviceId = deviceId;
        _deviceService = deviceService;
        _contentRepo = contentRepo;

        LoadSensorsCommand = ReactiveCommand.CreateFromTask(LoadSensorsAsync);
        AddSensorCommand = ReactiveCommand.CreateFromTask(AddSensorAsync);

        LoadSensorsCommand.Execute().Subscribe();
        LoadAvailableSensorTypes();
    }

    private async Task LoadSensorsAsync()
    {
        try
        {
            ErrorMessage = "";
            Sensors.Clear();
            var device = await _deviceService.GetDeviceDataAsync(_deviceId);

            foreach (var sensor in device.Sensors)
            {
                Sensors.Add(new SensorItemViewModel
                {
                    TypeId = sensor.TypeId,
                    SourceId = sensor.SourceId,
                    Name = sensor.Name.Value,
                    ParametersCount = sensor.Parameters.Count
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки сенсоров: {ex.Message}";
        }
    }

    private async void LoadAvailableSensorTypes()
    {
        try
        {
            var types = await _contentRepo.GetAllDefinitionsAsync();
            AvailableSensorTypes.Clear();

            foreach (var type in types)
            {
                AvailableSensorTypes.Add(type);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки типов сенсоров: {ex.Message}";
        }
    }

    private async Task AddSensorAsync()
    {
        try
        {
            ErrorMessage = "";

            if (SelectedSensorType == null)
            {
                ErrorMessage = "Тип сенсора не выбран";
                return;
            }

            await _deviceService.AddSensorWithParametersAsync(
                _deviceId,
                SelectedSensorType.TypeId,
                SourceId, // Используем введенный пользователем SourceId
                SelectedSensorType.Name.Value
            );

            await LoadSensorsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка добавления сенсора: {ex.Message}";
        }
    }
}

public class SensorItemViewModel
{
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string Name { get; set; }
    public int ParametersCount { get; set; }
}