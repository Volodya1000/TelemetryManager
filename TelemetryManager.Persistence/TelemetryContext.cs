using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TelemetryManager.Persistence.Entities.ContentEntities;
using TelemetryManager.Persistence.Entities.DeviceEntities;
using TelemetryManager.Persistence.Entities.PacketEntities;

namespace TelemetryManager.Persistence;

public class TelemetryContext : DbContext
{
    public DbSet<TelemetryPacketEntity> TelemetryPackets { get; set; }
    public DbSet<ContentItemEntity> ContentItems { get; set; }

    public DbSet<ContentDefinitionEntity> ContentDefinitions { get; set; } = null!;

    public DbSet<DeviceProfileEntity> DeviceProfiles { get; set; }

    public  TelemetryContext(DbContextOptions<TelemetryContext> options)
          : base(options){ }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}