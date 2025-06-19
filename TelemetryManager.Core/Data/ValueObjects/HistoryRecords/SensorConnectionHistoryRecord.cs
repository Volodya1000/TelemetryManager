namespace TelemetryManager.Core.Data.ValueObjects.HistoryRecords;

public record SensorConnectionHistoryRecord(DateTime Timestamp, bool IsConnected);