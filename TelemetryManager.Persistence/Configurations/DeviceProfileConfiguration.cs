using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.DeviceEntities;

namespace TelemetryManager.Persistence.Configurations;

public class DeviceProfileConfiguration : IEntityTypeConfiguration<DeviceProfileEntity>
{
    public void Configure(EntityTypeBuilder<DeviceProfileEntity> builder)
    {
        builder.HasKey(d => d.DeviceId);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(30);
        builder.Property(d => d.ActivationTime).IsRequired(false);

        builder.HasMany(d => d.Sensors)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SensorProfileConfiguration : IEntityTypeConfiguration<SensorProfileEntity>
{
    public void Configure(EntityTypeBuilder<SensorProfileEntity> builder)
    {
        builder.HasKey(s => new { s.DeviceId, s.TypeId, s.SourceId });
        builder.Property(s => s.Name).IsRequired().HasMaxLength(30);

        // Связи с зависимыми сущностями
        builder.HasMany(s => s.ConnectionHistory)
            .WithOne(ch => ch.Sensor)
            .HasForeignKey(ch => new { ch.DeviceId, ch.TypeId, ch.SourceId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Parameters)
            .WithOne(p => p.Sensor)
            .HasForeignKey(p => new { p.DeviceId, p.TypeId, p.SourceId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SensorParameterProfileConfiguration
      : IEntityTypeConfiguration<SensorParameterProfileEntity>
{
    public void Configure(EntityTypeBuilder<SensorParameterProfileEntity> builder)
    {
        builder.HasKey(p => new { p.DeviceId, p.TypeId, p.SourceId, p.ParameterName });

        builder.Property(p => p.ParameterName)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasOne(p => p.Sensor)
               .WithMany(s => s.Parameters)
               .HasForeignKey(p => new { p.DeviceId, p.TypeId, p.SourceId })
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ParameterDefinition)
               .WithMany(d => d.SensorParameters)
               .HasForeignKey(p => new { p.TypeId, p.ParameterName })
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ParameterIntervalChangeRecordConfiguration
      : IEntityTypeConfiguration<ParameterIntervalChangeRecordEntity>
{
    public void Configure(EntityTypeBuilder<ParameterIntervalChangeRecordEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ParameterName)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasOne(r => r.Parameter)
               .WithMany(p => p.IntervalHistory)
               .HasForeignKey(r => new
               {
                   r.DeviceId,
                   r.TypeId,
                   r.SourceId,
                   r.ParameterName
               })
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.ChangeTime).IsRequired();
        builder.Property(r => r.Min).IsRequired();
        builder.Property(r => r.Max).IsRequired();

        builder.HasIndex(r => r.ChangeTime);
    }


    public class SensorConnectionHistoryRecordConfiguration
    : IEntityTypeConfiguration<SensorConnectionHistoryRecordEntity>
    {
        public void Configure(EntityTypeBuilder<SensorConnectionHistoryRecordEntity> builder)
        {
            builder.HasKey(r => r.Id);

            // Составной внешний ключ
            builder.HasOne(r => r.Sensor)
                .WithMany(s => s.ConnectionHistory)
                .HasForeignKey(r => new { r.DeviceId, r.TypeId, r.SourceId })
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.Timestamp).IsRequired();
            builder.Property(r => r.IsConnected).IsRequired();
            builder.HasIndex(r => r.Timestamp);
        }
    }
}