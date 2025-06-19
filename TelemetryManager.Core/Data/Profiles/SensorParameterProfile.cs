using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public ParameterName Name { get; }
    public string Units { get; }
    public Interval CurrentInterval { get; private set; }

    private readonly List<ParameterIntervalChangeRecord> _intervalHistory = new();

    public IReadOnlyList<ParameterIntervalChangeRecord> IntervalHistory => _intervalHistory.AsReadOnly();

    public SensorParameterProfile(ParameterName name, string units, double min, double max)
    {
        Name = name;
        Units = units;
        CurrentInterval = new Interval(min, max);
        _intervalHistory.Add(new ParameterIntervalChangeRecord(DateTime.UtcNow, CurrentInterval));
    }

    public void SetInterval(double min, double max)
    {
        var newInterval = new Interval(min, max);
        CurrentInterval = newInterval;
        _intervalHistory.Add(new ParameterIntervalChangeRecord(DateTime.UtcNow, newInterval));
    }
}