using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.Data;

public enum ParsingErrorType
{
    SyncMarkerNotFound,      // Не удалось найти синхромаркер
    IncompleteHeader,        // Неполный заголовок пакета
    InvalidHeaderFormat,     // Некорректный формат заголовка
    SizeMismatch,            // Несоответствие размера пакета
    UnknownSensorType,       // Неизвестный тип датчика
    ContentReadFailed,       // Ошибка чтения содержимого
    ChecksumMismatch,        // Несовпадение контрольной суммы
    PaddingReadFailed,       // Ошибка чтения выравнивания
    DataValidationFailed     // Ошибка валидации данных
}

public record ParsingError(
    long StreamPosition,     // Позиция в потоке (байт)
    ParsingErrorType ErrorType, // Тип ошибки
    string Message,          // Детальное описание
    long PacketStartOffset,  // Смещение начала пакета
    ushort? DeviceId = null, // ID устройства (если известно)
    SensorType? SensorType = null // Тип датчика (если известно)
);