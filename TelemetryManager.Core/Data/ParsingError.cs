﻿namespace TelemetryManager.Core.Data;

public enum ParsingErrorType
{
    SyncMarkerNotFound,
    IncompleteHeader,
    InvalidHeaderFormat,
    SizeMismatch,
    UnknownSensorType,
    ContentReadFailed,
    ChecksumMismatch,
    PaddingReadFailed,
    DataValidationFailed,
    UnknownDeviceId,
    DeviceDontContainsSensorWithSuchSourceId
}

public record ParsingError(
    long PacketStartOffset,
    ParsingErrorType ErrorType,
    string Message,
    uint? Time = null,
    ushort? DeviceId = null,
    byte? SensorType = null,
    byte? SourceId = null,
    ushort? Size = null
);