using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TelemetryManager.Persistence.Entities;

namespace TelemetryManager.Persistence;

public class TelemetryContext : DbContext
{
    public DbSet<TelemetryPacketEntity> TelemetryPackets => Set<TelemetryPacketEntity>();
    public DbSet<ContentItemEntity> ContentItems => Set<ContentItemEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=TelemetryManagerSqliteDataBase.db;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}