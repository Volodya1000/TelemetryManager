using TelemetryManager.Core.Data.ValueObjects;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public ParametrName Name { get; }
    public string Units { get; }
    public Interval ValueRange { get; private set; }

    public SensorParameterProfile(ParametrName name, string units, double min, double max)
    {
        Name = name;
        Units = units;
        ValueRange = new Interval(min, max);
    }

    public void SetMinValue(double newMin) =>
        ValueRange = new Interval(newMin, ValueRange.Max);

    public void SetMaxValue(double newMax) =>
        ValueRange = new Interval(ValueRange.Min, newMax);

    public void SetInterval(double newMin, double newMax) =>
        ValueRange = new Interval(newMin, newMax);
}