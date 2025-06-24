using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class SensorParameterItemViewModel : ReactiveObject
{

    public string Name { get; }
    public string IntervalDisplay { get; }
    public Interval CurrentInterval { get; }

    public SensorItemViewModel ParentSensor { get; }

    public ReactiveCommand<Unit, Unit> EditIntervalCommand { get; }

    public SensorParameterItemViewModel(
        SensorItemViewModel parentSensor,
        SensorParameterProfile parameter,
        Func<SensorParameterItemViewModel, Task> editCallback)
    {
        ParentSensor= parentSensor;
        Name = parameter.Name.Value;
        CurrentInterval = parameter.CurrentInterval;
        IntervalDisplay = $"[{CurrentInterval.Min}, {CurrentInterval.Max}]";

        EditIntervalCommand = ReactiveCommand.CreateFromTask(async () => {
            await editCallback(this);
        });
    }
}