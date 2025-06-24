using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class DeviceSensorsViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

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
    public ReactiveCommand<Unit, Unit> LoadAvailableSensorTypesCommand { get; }

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

        LoadSensorsCommand.Execute().Subscribe().DisposeWith(_disposables);


        LoadAvailableSensorTypesCommand = ReactiveCommand.CreateFromTask(LoadAvailableSensorTypesAsync);

        // Запускаем начальную загрузку
        LoadAvailableSensorTypesCommand.Execute().Subscribe().DisposeWith(_disposables);
    }


    private async Task LoadAvailableSensorTypesAsync()
    {
        try
        {
            ErrorMessage = "";
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
            this.Log().Error(ex, "Failed to load sensor types");
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
                SourceId,
                SelectedSensorType.Name.Value
            );

            await LoadSensorsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка добавления сенсора: {ex.Message}";
        }
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
                var isConnected =  device.IsSensorConnectedAt(
                    new SensorId(sensor.TypeId, sensor.SourceId),
                    DateTime.Now
                );

                var sensorVM = new SensorItemViewModel(
                    _deviceId,
                    UpdateSensorConnection,
                    sensor.TypeId,
                    sensor.SourceId,
                    sensor.Name.Value,
                    sensor.Parameters.Count,
                    isConnected
                );

                Sensors.Add(sensorVM);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки сенсоров: {ex.Message}";
        }
    }

    private async Task UpdateSensorConnection(
        ushort deviceId,
        byte typeId,
        byte sourceId,
        bool connect)
    {
        try
        {
            var timestamp = DateTime.Now;
            if (connect)
            {
                await _deviceService.MarkSensorConnectedAsync( // Исправлено!
                      deviceId, typeId, sourceId, timestamp);
            }
            else
            {
                await _deviceService.MarkSensorDisconnectedAsync( // Исправлено!
                 deviceId, typeId, sourceId, timestamp);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка изменения состояния: {ex.Message}";
            throw; // Для обработки в ToggleConnectionCommand
        }
    }

    public void Dispose() => _disposables.Dispose();
}

