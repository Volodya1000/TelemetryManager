namespace TelemetryManager.Core.Data;

public record SensorParametrSnapshot(
    string Name,
    double Value,
    double MinValue,
    double MaxValue)
{
    public bool IsAnomalous => Value < MinValue || Value > MaxValue;
}
