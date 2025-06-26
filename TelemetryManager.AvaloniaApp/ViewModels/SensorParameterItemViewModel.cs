using ReactiveUI;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class SensorParameterItemViewModel : ReactiveObject
{
    public string Name { get; }
    public string IntervalDisplay { get; }

    public SensorParameterItemViewModel(SensorParameterProfile parameter)
    {
        Name = parameter.Name.Value;
        IntervalDisplay = $"[{parameter.CurrentInterval.Min}, {parameter.CurrentInterval.Max}]";
    }
}