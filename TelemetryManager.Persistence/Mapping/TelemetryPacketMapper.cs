using TelemetryManager.Core.Data;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Persistence.Entities;

namespace TelemetryManager.Persistence.Mapping;

public static class TelemetryPacketMapper
{
    public static TelemetryPacketWithDate MapToDto(TelemetryPacketEntity entity)
    {
        return new TelemetryPacketWithDate(
            DateTimeOfSending: entity.Time,
            DevId: entity.DevId,
            SensorId: new SensorId(entity.SensorType, entity.SensorSourceId),
            Content: entity.ContentItems.ToDictionary(i => i.Key, i => i.Value)
        );
    }

    public static List<TelemetryPacketWithDate> MapToDtos(List<TelemetryPacketEntity> entities)
    {
        return entities.Select(MapToDto).ToList();
    }
}