namespace TelemetryManager.Core.Data.ValueObjects;

public sealed record Interval
{
    public double Min { get; }
    public double Max { get; }

    public Interval(double min, double max)
    {
        if (min >= max)
            throw new ArgumentException($"Min value {min} must be less than max {max}");

        Min = min;
        Max = max;
    }

    public bool Contains(double value) => value >= Min && value <= Max;

    public override string ToString() => $"[{Min}, {Max}]";
}