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

        builder.HasMany(d => d.Sensors)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// SensorProfileConfiguration.cs
public class SensorProfileConfiguration : IEntityTypeConfiguration<SensorProfileEntity>
{
    public void Configure(EntityTypeBuilder<SensorProfileEntity> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(30);

        // Уникальный индекс для (DeviceId, TypeId, SourceId)
        builder.HasIndex(s => new { s.DeviceId, s.TypeId, s.SourceId }).IsUnique();

        builder.HasMany(s => s.ConnectionHistory)
            .WithOne()
            .HasForeignKey(h => h.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Parameters)
            .WithOne(p => p.Sensor)
            .HasForeignKey(p => p.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// SensorParameterProfileConfiguration.cs
public class SensorParameterProfileConfiguration : IEntityTypeConfiguration<SensorParameterProfileEntity>
{
    public void Configure(EntityTypeBuilder<SensorParameterProfileEntity> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ParameterName).IsRequired().HasMaxLength(20);

        // Уникальный индекс для (SensorId, ParameterName)
        builder.HasIndex(p => new { p.SensorId, p.ParameterName }).IsUnique();

        builder.HasMany(p => p.IntervalHistory)
            .WithOne()
            .HasForeignKey(h => h.ParameterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParameterIntervalChangeRecordConfiguration
    : IEntityTypeConfiguration<ParameterIntervalChangeRecordEntity>
{
    public void Configure(EntityTypeBuilder<ParameterIntervalChangeRecordEntity> builder)
    {
        // Первичный ключ
        builder.HasKey(r => r.Id);

        // Настройка связи с SensorParameterProfileEntity
        builder.HasOne(r => r.Parameter)
            .WithMany(p => p.IntervalHistory)
            .HasForeignKey(r => r.ParameterId)
            .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении параметра

        // Настройка индексов
        builder.HasIndex(r => r.ParameterId); // Для быстрого поиска по параметру
        builder.HasIndex(r => r.ChangeTime);  // Для временных запросов

        // Настройка свойств
        builder.Property(r => r.ChangeTime)
            .IsRequired();

        builder.Property(r => r.Min)
            .IsRequired()
            .HasColumnType("double precision");

        builder.Property(r => r.Max)
            .IsRequired()
            .HasColumnType("double precision");
    }
}

public class SensorConnectionHistoryRecordConfiguration
    : IEntityTypeConfiguration<SensorConnectionHistoryRecordEntity>
{
    public void Configure(EntityTypeBuilder<SensorConnectionHistoryRecordEntity> builder)
    {
        // Первичный ключ
        builder.HasKey(r => r.Id);

        // Настройка связи с SensorProfileEntity
        builder.HasOne(r => r.Sensor)
            .WithMany(s => s.ConnectionHistory)
            .HasForeignKey(r => r.SensorId)
            .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении сенсора

        // Настройка индексов
        builder.HasIndex(r => r.SensorId);    // Для быстрого поиска по сенсору
        builder.HasIndex(r => r.Timestamp);  // Для временных запросов
        builder.HasIndex(r => r.IsConnected); // Для фильтрации по статусу

        // Настройка свойств
        builder.Property(r => r.Timestamp)
            .IsRequired();

        builder.Property(r => r.IsConnected)
            .IsRequired();
    }
}