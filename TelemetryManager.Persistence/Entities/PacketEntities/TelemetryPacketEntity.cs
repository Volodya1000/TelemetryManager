﻿namespace TelemetryManager.Persistence.Entities.PacketEntities;

public class TelemetryPacketEntity
{
    public int Id { get; set; }
    public DateTime Time { get; set; } 
    public ushort DevId { get; set; }
    public byte SensorType { get; set; }
    public byte SensorSourceId { get; set; }
    public List<ContentItemEntity> ContentItems { get; set; } = new();
}