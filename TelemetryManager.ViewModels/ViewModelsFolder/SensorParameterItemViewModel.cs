using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Globalization;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class SensorParameterItemViewModel: ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly DeviceService _deviceService;
    private readonly ushort _deviceId;
    private readonly byte _typeId;
    private readonly byte _sensorId;
    private readonly string _parameterName;

    [Reactive] public string Name { get; private set; }
    [Reactive] public double MinValue { get; private set; }
    [Reactive] public double MaxValue { get; private set; }
    [Reactive] public bool IsEditing { get; private set; }
    [Reactive] public string EditableMinValue { get; set; }
    [Reactive] public string EditableMaxValue { get; set; }

    private readonly ObservableAsPropertyHelper<string> _intervalDisplay;
    public string IntervalDisplay => _intervalDisplay.Value;

    public ReactiveCommand<Unit, Unit> StartEditCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadIntervalCommand { get; }

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

        this.WhenAnyValue(x => x.MinValue, x => x.MaxValue)
          .Select(values => $"[{values.Item1:F2} - {values.Item2:F2}]")
          .ToProperty(this, x => x.IntervalDisplay, out _intervalDisplay)
          .DisposeWith(_disposables);

        SaveCommand = ReactiveCommand.CreateFromTask(
            Save,
            this.WhenAnyValue(
                x => x.EditableMinValue,
                x => x.EditableMaxValue,
                (min, max) => IsValidDouble(min) && IsValidDouble(max) &&
                             TryParseDouble(min) < TryParseDouble(max)));

        LoadIntervalCommand = ReactiveCommand.CreateFromTask(LoadCurrentIntervalAsync);

        // Подписка на изменения MinValue и MaxValue для обновления IntervalDisplay
        this.WhenAnyValue(x => x.MinValue, x => x.MaxValue)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IntervalDisplay)));

        // Загрузка начальных значений
        LoadIntervalCommand.Execute().Subscribe();
    }

    private async Task LoadCurrentIntervalAsync()
    {
        try
        {
            var interval = await _deviceService
                .GetParameterInterval(_deviceId, _typeId, _sensorId, _parameterName)
                .ConfigureAwait(false);

            RxApp.MainThreadScheduler.Schedule(() =>
            {
                MinValue = interval.currentMin;
                MaxValue = interval.currentMax;
                EditableMinValue = MinValue.ToString("F2", CultureInfo.InvariantCulture);
                EditableMaxValue = MaxValue.ToString("F2", CultureInfo.InvariantCulture);
            });
        }
        catch (Exception ex)
        {
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
            var newMin = TryParseDouble(EditableMinValue);
            var newMax = TryParseDouble(EditableMaxValue);

            await _deviceService
                .SetParameterIntervalAsync(_deviceId, _typeId, _sensorId, _parameterName, newMin, newMax)
                .ConfigureAwait(false);

            MinValue = newMin;
            MaxValue = newMax;
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

    public void Dispose() => _disposables?.Dispose();
}