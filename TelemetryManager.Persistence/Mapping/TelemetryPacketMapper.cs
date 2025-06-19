using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Persistence.Entities;

namespace TelemetryManager.Persistence.Mapping;

public static class TelemetryPacketMapper
{
    public static TelemetryPacket MapToDto(TelemetryPacketEntity entity)
    {
        return new TelemetryPacket(
            DateTimeOfSending: entity.Time,
            DevId: entity.DevId,
            SensorId: new SensorId(entity.SensorType, entity.SensorSourceId),
            Content: entity.ContentItems.ToDictionary(i => i.Key, i => i.Value)
        );
    }

    public static List<TelemetryPacket> MapToDtos(List<TelemetryPacketEntity> entities)
    {
        return entities.Select(MapToDto).ToList();
    }
    public static TelemetryPacketEntity MapToEntity(TelemetryPacket packet)
    {
        return new TelemetryPacketEntity
        {
            Time = packet.DateTimeOfSending,
            DevId = packet.DevId,
            SensorType = packet.SensorId.TypeId,
            SensorSourceId = packet.SensorId.SourceId,
            ContentItems = packet.Content
                .Select(kv => new ContentItemEntity { Key = kv.Key, Value = kv.Value })
                .ToList()
        };
    }
}