using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Globalization;
using System.Reactive;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.ViewModels.ViewModelsFolder;
public class SensorParameterItemViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private readonly ushort _deviceId;
    private readonly byte _typeId;
    private readonly byte _sensorId;
    private readonly string _parameterName;

    private string _editableMinValue;
    private string _editableMaxValue;

    public SensorParameterItemViewModel(
        ushort deviceId,
        byte typeId,
        byte sensorId,
        string parameterName,
        DeviceService deviceService)
    {
        _deviceId = deviceId;
        _typeId = typeId;
        _sensorId = sensorId;
        _parameterName = parameterName;
        _deviceService = deviceService;

        Name = parameterName;

        // Инициализация команд
        StartEditCommand = ReactiveCommand.Create(StartEdit);
        CancelCommand = ReactiveCommand.Create(Cancel);

        SaveCommand = ReactiveCommand.CreateFromTask(
            Save,
            this.WhenAnyValue(
                x => x.EditableMinValue,
                x => x.EditableMaxValue,
                (min, max) => IsValidDouble(min) && IsValidDouble(max) &&
                             TryParseDouble(min) < TryParseDouble(max)));

        LoadIntervalCommand = ReactiveCommand.CreateFromTask(LoadCurrentIntervalAsync);

        // Загрузка начальных значений
        LoadIntervalCommand.Execute().Subscribe();
    }

    [Reactive]
    public string Name { get; private set; }

    [Reactive]
    public double MinValue { get; private set; }

    [Reactive]
    public double MaxValue { get; private set; }

    [Reactive]
    public bool IsEditing { get; private set; }

    public string EditableMinValue
    {
        get => _editableMinValue;
        set
        {
            if (IsValidDouble(value))
            {
                this.RaiseAndSetIfChanged(ref _editableMinValue, value);
            }
        }
    }

    public string EditableMaxValue
    {
        get => _editableMaxValue;
        set
        {
            if (IsValidDouble(value))
            {
                this.RaiseAndSetIfChanged(ref _editableMaxValue, value);
            }
        }
    }

    public string IntervalDisplay => $"[{MinValue:F2} - {MaxValue:F2}]";

    // Команды
    public ReactiveCommand<Unit, Unit> StartEditCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadIntervalCommand { get; }

    private async Task LoadCurrentIntervalAsync()
    {
        try
        {
            var interval = await _deviceService
                .GetParameterInterval(_deviceId, _typeId, _sensorId, _parameterName)
                .ConfigureAwait(false);

            MinValue = interval.currentMin;
            MaxValue = interval.currentMax;

            // Обновляем редактируемые значения
            EditableMinValue = MinValue.ToString("F2", CultureInfo.InvariantCulture);
            EditableMaxValue = MaxValue.ToString("F2", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Error loading interval: {ex.Message}");
        }
    }

    private void StartEdit()
    {
        EditableMinValue = MinValue.ToString("F2", CultureInfo.InvariantCulture);
        EditableMaxValue = MaxValue.ToString("F2", CultureInfo.InvariantCulture);
        IsEditing = true;
    }

    private async Task Save()
    {
        try
        {
            MinValue = TryParseDouble(EditableMinValue);
            MaxValue = TryParseDouble(EditableMaxValue);

            // Корректировка если Min > Max
            if (MinValue > MaxValue)
            {
                (MinValue, MaxValue) = (MaxValue, MinValue);
            }

            await _deviceService
                .SetParameterIntervalAsync(_deviceId, _typeId, _sensorId, _parameterName, MinValue, MaxValue)
                .ConfigureAwait(false);

            IsEditing = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving interval: {ex.Message}");
            throw;
        }
    }

    private void Cancel()
    {
        IsEditing = false;
    }

    private bool IsValidDouble(string value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    private double TryParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return 0.0;
    }
}