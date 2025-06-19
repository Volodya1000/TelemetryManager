using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.EventsArgs;

public class ParameterOutOfRangeEventArgs : EventArgs
{
    public ushort DeviceId { get; }
    public SensorId SensorId { get; }
    public string ParameterName { get; }
    public double CurrentValue { get; }
    public double MinValue { get; }
    public double MaxValue { get; }
    public double Deviation { get; }

    public ParameterOutOfRangeEventArgs(
        ushort deviceId,
        SensorId sensorId,
        string parameterName,
        double currentValue,
        double minValue,
        double maxValue,
        double deviation)
    {
        DeviceId = deviceId;
        SensorId = sensorId;
        ParameterName = parameterName;
        CurrentValue = currentValue;
        MinValue = minValue;
        MaxValue = maxValue;
        Deviation = deviation;
    }

    public ParameterOutOfRangeEventArgs(
        ushort deviceId,
        SensorId sensorId,
        string parameterName,
        double currentValue,
        double minValue,
        double maxValue)
        : this(
            deviceId,
            sensorId,
            parameterName,
            currentValue,
            minValue,
            maxValue,
            CalculateDeviation(currentValue, minValue, maxValue))
    {
    }

    public bool IsBelowMin => Deviation < 0;
    public bool IsAboveMax => Deviation > 0;
    public double AbsoluteDeviation => Math.Abs(Deviation);

    private static double CalculateDeviation(double value, double min, double max)
    {
        if (value < min) return min - value;
        if (value > max) return value - max;
        return 0;
    }

    public override string ToString()
    {
        var direction = IsBelowMin ? "below" : "above";
        return $"{ParameterName} value {CurrentValue} is {AbsoluteDeviation} {direction} allowed range [{MinValue}-{MaxValue}]";
    }
}