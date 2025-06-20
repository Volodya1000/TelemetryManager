using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public ParameterDefinition Definition { get; }

    public Interval CurrentInterval { get; private set; }

    private readonly List<ParameterIntervalChangeRecord> _intervalHistory = new();

    public IReadOnlyList<ParameterIntervalChangeRecord> IntervalHistory => _intervalHistory.AsReadOnly();

    public SensorParameterProfile(ParameterDefinition definition,double min, double max)
    {
        CurrentInterval = new Interval(min, max);
        Definition = definition;
        _intervalHistory.Add(new ParameterIntervalChangeRecord(DateTime.UtcNow, CurrentInterval));
    }

    public void SetInterval(double min, double max)
    {
        var newInterval = new Interval(min, max);
        CurrentInterval = newInterval;
        _intervalHistory.Add(new ParameterIntervalChangeRecord(DateTime.UtcNow, newInterval));
    }
}