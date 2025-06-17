namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public string Name { get; }
    public string Units { get; }
    public double Min { get; private set; }
    public double Max { get; private set; }

    public SensorParameterProfile(string name, string units, double min, double max)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Sensor parameter name cannot be null or empty.");

        ValidateInterval(min, max, nameof(min));
        Name = name;
        Units = units;
        Min = min;
        Max = max;
    }

    public void SetMinValue(double newMinValue)
    {
        ValidateInterval(newMinValue, Max, nameof(newMinValue));
        Min = newMinValue;
    }

    public void SetMaxValue(double newMaxValue)
    {
        ValidateInterval(Min, newMaxValue, nameof(newMaxValue));
        Max = newMaxValue;
    }

    public void SetInterval(double newMinValue, double newMaxValue)
    {
        ValidateInterval(newMinValue, newMaxValue, nameof(newMinValue));
        Min = newMinValue;
        Max = newMaxValue;
    }

    private static void ValidateInterval(double min, double max, string paramName)
    {
        if (min >= max)
            throw new ArgumentException($"'{paramName}': Minimum value must be less than maximum value.", paramName);
    }
}