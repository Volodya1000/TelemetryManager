using ReactiveUI;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class EditIntervalViewModel : ReactiveObject
{
    public string ParameterName { get; }
    public double CurrentMin { get; }
    public double CurrentMax { get; }

    private double _newMin;
    public double NewMin
    {
        get => _newMin;
        set => this.RaiseAndSetIfChanged(ref _newMin, value);
    }

    private double _newMax;
    public double NewMax
    {
        get => _newMax;
        set => this.RaiseAndSetIfChanged(ref _newMax, value);
    }

    public EditIntervalViewModel(string parameterName, double currentMin, double currentMax)
    {
        ParameterName = parameterName;
        CurrentMin = currentMin;
        CurrentMax = currentMax;
        NewMin = currentMin;
        NewMax = currentMax;
    }
}