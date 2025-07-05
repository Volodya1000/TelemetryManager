using System.Collections.ObjectModel;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorParameterProfile
{
    public ParameterName Name { get; }

    public Interval CurrentInterval => _intervalHistory.Last().Interval;

    private readonly Collection<ParameterIntervalChangeRecord> _intervalHistory;

    public IReadOnlyCollection<ParameterIntervalChangeRecord> IntervalHistory => _intervalHistory.AsReadOnly();

    public SensorParameterProfile(ParameterName name, Interval initialInterval, DateTime initialTimestamp)
    {
        Name = name;
        _intervalHistory = new Collection<ParameterIntervalChangeRecord>
        {
            new ParameterIntervalChangeRecord(initialTimestamp, initialInterval)
        };
    }

    public void SetInterval(Interval newInterval, DateTime timestamp)
    {
        if (timestamp < _intervalHistory.Last().ChangeTime)
        {
            throw new ArgumentException("New interval timestamp cannot be earlier than last record timestamp");
        }

        _intervalHistory.Add(new ParameterIntervalChangeRecord(timestamp, newInterval));
    }

    public Interval GetIntervalForDate(DateTime date)
    {
        return _intervalHistory
            .LastOrDefault(record => record.ChangeTime <= date)?.Interval
            ?? _intervalHistory.First().Interval;
    }
}