using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.Identifiers;

//TypeId нужен внутри SensorId так как по заданию  идентификатор датчика должен быть уникальным для каждого TypeId

//Автоматическая реализация IEquatable<T> Equals() GetHashCode() == и != о
public readonly record struct SensorId(SensorType TypeId, byte SourceId);
