namespace TelemetryManager.Core.Identifiers;

//TypeId нужен внутри SensorId так как по заданию  идентификатор датчика должен быть уникальным для каждого TypeId
public readonly record struct SensorId(ushort DeviceId, byte TypeId, byte SourceId);
