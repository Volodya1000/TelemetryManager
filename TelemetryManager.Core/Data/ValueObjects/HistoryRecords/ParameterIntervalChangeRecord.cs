namespace TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

public record ParameterIntervalChangeRecord(DateTime ChangeTime, Interval Interval);