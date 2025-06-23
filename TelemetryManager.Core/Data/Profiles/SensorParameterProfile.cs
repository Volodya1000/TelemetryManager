using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public ParameterName Name { get; }

    public Interval CurrentInterval { get; private set; }

    public SensorParameterProfile(ParameterName name, Interval interval)
    {
        Name = name;
        CurrentInterval = interval;
    }

    private readonly List<ParameterIntervalChangeRecord> _intervalHistory = new();

    public IReadOnlyList<ParameterIntervalChangeRecord> IntervalHistory => _intervalHistory.AsReadOnly();


    public void SetInterval(Interval newInterval, DateTime timestamp)
    {
        CurrentInterval = newInterval;
        _intervalHistory.Add(new ParameterIntervalChangeRecord(timestamp, newInterval));
    }
}