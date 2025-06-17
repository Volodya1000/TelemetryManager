using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.Data;

public enum ParsingErrorType
{
    IncompletePacketHeader,
    InvalidPacketFormat,
    InvalidPacketSize,
    ContentReadFailed,
    PaddingReadFailed,
    ChecksumMismatch,
    DataValidationFailed,
    UnexpectedStreamError
}

public record ParsingError(
        long StreamPosition,
        ParsingErrorType ErrorType,
        string Message,
        long PacketStartOffset,
        long PacketEndOffset,
        byte[] RawData = null
    );