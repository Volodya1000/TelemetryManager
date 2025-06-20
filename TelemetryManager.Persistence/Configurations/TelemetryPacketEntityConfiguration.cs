using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.PacketEntities;

namespace TelemetryManager.Persistence.Configurations;
public class TelemetryPacketEntityConfiguration : IEntityTypeConfiguration<TelemetryPacketEntity>
{
    public void Configure(EntityTypeBuilder<TelemetryPacketEntity> builder)
    {
        builder.ToTable("TelemetryPackets");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Time)
            .IsRequired()
            .HasColumnType("TEXT"); 

        builder.Property(p => p.DevId)
            .HasColumnType("INTEGER"); 

        builder.Property(p => p.SensorType)
            .HasConversion<byte>()
            .HasColumnType("INTEGER");

        builder.Property(p => p.SensorSourceId)
            .HasColumnType("INTEGER");

        builder.HasMany(p => p.ContentItems)
            .WithOne(i => i.Packet)
            .HasForeignKey(i => i.TelemetryPacketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Time); 
    }
}
