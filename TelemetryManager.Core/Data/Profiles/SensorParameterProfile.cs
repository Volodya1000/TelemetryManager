namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public string Name { get; }
    public string Units { get; }
    public double MinValue { get; private set; } 
    public double MaxValue { get; private set; }

    public SensorParameterProfile(string name, string units, double minValue, double maxValue)
    {
        ValidateInterval(minValue, maxValue);
        Name = name;
        Units = units;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public void SetMinValue(double newMinValue)
    {
        ValidateInterval(newMinValue, MaxValue);
        MinValue = newMinValue;
    }

    public void SetMaxValue(double newMaxValue)
    {
        ValidateInterval(MinValue, newMaxValue);
        MaxValue = newMaxValue;
    }

    public void SetInterval(double newMinValue, double newMaxValue)
    {
        ValidateInterval(newMinValue, newMaxValue);
        MinValue = newMinValue;
        MaxValue = newMaxValue;
    }

    private static void ValidateInterval(double min, double max)
    {
        if (min >= max)
            throw new ArgumentException("MinValue must be less than MaxValue.");
    }
}