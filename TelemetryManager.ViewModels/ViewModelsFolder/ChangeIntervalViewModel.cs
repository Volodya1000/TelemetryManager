using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class ChangeIntervalViewModel : ReactiveObject
{
    private readonly DeviceService _deviceService;
    private readonly ushort _deviceId;
    private readonly SensorId _sensorId;
    private readonly string _parameterName;

    [Reactive] public double Min { get; set; }
    [Reactive] public double Max { get; set; }
    public string Message { get; }

    public ReactiveCommand<Unit, Unit> SaveChangedInterval { get; }
    public ReactiveCommand<Unit, (double Min, double Max)> LoadIntervalCommand { get; }

    public ChangeIntervalViewModel(ushort deviceId, SensorId sensorId, string parameterName, DeviceService deviceService)
    {
        _deviceId = deviceId;
        _sensorId = sensorId;
        _parameterName = parameterName;
        Message = $"Введите интервал для параметра \"{parameterName}\"";
        _deviceService = deviceService;

        LoadIntervalCommand = ReactiveCommand.CreateFromTask(LoadCurrentIntervalAsync);

        SaveChangedInterval = ReactiveCommand.CreateFromTask(ChangeIntervalAsync);

        LoadIntervalCommand.Subscribe(result =>
        {
            Min = result.Min;
            Max = result.Max;
        });

        LoadIntervalCommand.Execute().Subscribe();
    }

    private async Task<(double Min, double Max)> LoadCurrentIntervalAsync()
    {
        return await _deviceService.GetParameterInterval(_deviceId, _sensorId, _parameterName);
    }

    private async Task ChangeIntervalAsync()
    {
        if (Min >= Max)
        {
            return;
        }

        await _deviceService.SetParameterIntervalAsync(_deviceId, _sensorId, _parameterName, Min, Max);

        LoadIntervalCommand.Execute().Subscribe();
    }
}