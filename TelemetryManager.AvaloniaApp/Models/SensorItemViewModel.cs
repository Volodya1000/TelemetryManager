namespace TelemetryManager.AvaloniaApp.Models;

public class SensorItemViewModel
{
    public byte TypeId { get; set; }
    public byte SourceId { get; set; }
    public string Name { get; set; }
    public int ParametersCount { get; set; }
}