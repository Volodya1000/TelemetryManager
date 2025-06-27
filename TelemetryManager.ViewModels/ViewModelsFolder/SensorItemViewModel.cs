using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class SensorItemViewModel : ReactiveObject
{
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string Name { get; set; }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleConnectionCommand { get; }

    public ObservableCollection<SensorParameterItemViewModel> Parameters { get; } = new();

    public SensorItemViewModel(
     ushort deviceId,
     Func<ushort, byte, byte, bool, Task> updateConnection,
     byte typeId,
     byte sourceId,
     string name,
     bool isConnected,
     IEnumerable<SensorParameterProfile> parameters)
    {
        TypeId = typeId;
        SourceId = sourceId;
        Name = name;
        _isConnected = isConnected;

        foreach (var param in parameters)
        {
            Parameters.Add(new SensorParameterItemViewModel(param));
        }

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