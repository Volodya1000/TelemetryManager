using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorId Id { get; }
    public byte TypeId => Id.TypeId;
    public byte SourceId => Id.SourceId;
    public Name Name { get; init; }

    private readonly Dictionary<ParameterName, SensorParameterProfile> _parametersDict;
    public IReadOnlyList<SensorParameterProfile> Parameters => _parametersDict.Values.ToList();

    private readonly List<SensorConnectionHistoryRecord> _connectionHistory = new();
    public IReadOnlyList<SensorConnectionHistoryRecord> ConnectionHistory => _connectionHistory.AsReadOnly();

    public SensorProfile(SensorId id, Name name, IReadOnlyList<SensorParameterProfile> parameters)
    {
        Id = id;
        Name = name;

        _parametersDict = parameters.ToDictionary(p => p.Name);
        if (_parametersDict.Count != parameters.Count)
            throw new ArgumentException("Duplicate parameter names found");
    }

    public SensorParameterProfile GetParameter(ParameterName name)
    {
        if (_parametersDict.TryGetValue(name, out var parameter))
            return parameter;

        throw new KeyNotFoundException($"Parameter '{name}' not found in sensor {Id}");
    }


    public void MarkConnected(DateTime timestamp)
    {
        ValidateTimestampOrder(timestamp);
        ValidateConnectionStateTransition(true);

        _connectionHistory.Add(new SensorConnectionHistoryRecord(timestamp, true));
    }

    public void MarkDisconnected(DateTime timestamp)
    {
        ValidateTimestampOrder(timestamp);
        ValidateConnectionStateTransition(false);

        _connectionHistory.Add(new SensorConnectionHistoryRecord(timestamp, false));
    }

    private void ValidateTimestampOrder(DateTime newTimestamp)
    {
        if (_connectionHistory.Count > 0)
        {
            var lastRecord = _connectionHistory[^1];
            if (newTimestamp <= lastRecord.Timestamp)
            {
                throw new InvalidOperationException(
                    $"New event timestamp ({newTimestamp}) must be after last event timestamp ({lastRecord.Timestamp})");
            }
        }
    }

    private void ValidateConnectionStateTransition(bool newState)
    {
        if (_connectionHistory.Count == 0) return;

        var lastState = _connectionHistory[^1].IsConnected;

        if (lastState == newState)
        {
            throw new InvalidOperationException(
                $"Cannot mark sensor as {(newState ? "connected" : "disconnected")} " +
                $"when it's already {(lastState ? "connected" : "disconnected")}");
        }
    }

    public bool IsConnectedAt(DateTime timestamp)
    {
        var lastRecord = _connectionHistory
            .Where(r => r.Timestamp <= timestamp)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefault();

        return lastRecord?.IsConnected ?? false;
    }
}
