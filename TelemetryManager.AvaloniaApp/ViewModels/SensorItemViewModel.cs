using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class SensorItemViewModel : ReactiveObject
{
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string Name { get; set; }
    public int ParametersCount { get; set; }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleConnectionCommand { get; }

    public SensorItemViewModel(
     ushort deviceId,
     Func<ushort, byte, byte, bool, Task> updateConnection,
     byte typeId,
     byte sourceId,
     string name,
     int parametersCount,
     bool isConnected)
    {
        TypeId = typeId;
        SourceId = sourceId;
        Name = name;
        ParametersCount = parametersCount;
        _isConnected = isConnected;

        this.WhenAnyValue(x => x.IsConnected)
        .Skip(1) // Пропустить начальное значение
        .Subscribe(async connected =>
        {
            try
            {
                await updateConnection(deviceId, TypeId, SourceId, connected);
            }
            catch (Exception ex)
            {
                IsConnected = !connected;
            }
        });
    }
}