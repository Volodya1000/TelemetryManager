using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;
using DialogHostAvalonia;
using System.Reactive.Threading.Tasks;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;

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

    // Interaction для показа диалога редактирования интервала
    public Interaction<EditIntervalViewModel, Interval?> ShowEditIntervalDialog { get; } = new();

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
        LoadAvailableSensorTypesCommand = ReactiveCommand.CreateFromTask(LoadAvailableSensorTypesAsync);

        // Регистрация обработчика для диалога
        ShowEditIntervalDialog
      .RegisterHandler(new Action<InteractionContext<EditIntervalViewModel, Interval?>>(async interaction =>
      {
          await DoShowEditIntervalDialogAsync(interaction);
      }))
      .DisposeWith(_disposables);

        // Запуск начальной загрузки
        LoadSensorsCommand.Execute().Subscribe().DisposeWith(_disposables);
        LoadAvailableSensorTypesCommand.Execute().Subscribe().DisposeWith(_disposables);
    }

    private async Task DoShowEditIntervalDialogAsync(InteractionContext<EditIntervalViewModel, Interval?> interaction)
    {
        try
        {
            var dialog = new EditIntervalDialog
            {
                DataContext = interaction.Input
            };

            var result = await DialogHost.Show(dialog, "MainDialogHost");
            interaction.SetOutput(result as Interval?);
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, "Error showing interval dialog");
            interaction.SetOutput(null);
        }
    }

    private async Task EditParameterAsync(SensorParameterItemViewModel parameter)
    {
        try
        {
            ErrorMessage = "";

            var editVm = new EditIntervalViewModel(
                parameter.Name,
                parameter.CurrentInterval.Min,
                parameter.CurrentInterval.Max
            );

            var newInterval = await ShowEditIntervalDialog.Handle(editVm);

            if (newInterval!=null)
            {
                await _deviceService.SetParameterIntervalAsync(
                    _deviceId,
                    parameter.ParentSensor.TypeId,
                    parameter.ParentSensor.SourceId,
                    parameter.Name,
                    newInterval.Min,
                    newInterval.Max
                );

                // Обновляем данные
                await LoadSensorsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка изменения интервала: {ex.Message}";
            this.Log().Error(ex, "Failed to edit parameter interval");
        }
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
            this.Log().Error(ex, "Failed to add sensor");
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
                var isConnected = device.IsSensorConnectedAt(
                    new SensorId(sensor.TypeId, sensor.SourceId),
                    DateTime.Now
                );

                var sensorVM = new SensorItemViewModel(
                    _deviceId,
                    UpdateSensorConnection,
                    sensor.TypeId,
                    sensor.SourceId,
                    sensor.Name.Value,
                    isConnected,
                    sensor.Parameters,
                    EditParameterAsync
                );

                Sensors.Add(sensorVM);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки сенсоров: {ex.Message}";
            this.Log().Error(ex, "Failed to load sensors");
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
                await _deviceService.MarkSensorConnectedAsync(
                    deviceId, typeId, sourceId, timestamp);
            }
            else
            {
                await _deviceService.MarkSensorDisconnectedAsync(
                    deviceId, typeId, sourceId, timestamp);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка изменения состояния: {ex.Message}";
            this.Log().Error(ex, "Failed to update sensor connection");
            throw;
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
