using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.PacketEntities;

namespace TelemetryManager.Persistence.Configurations;

public class ContentItemEntityConfiguration : IEntityTypeConfiguration<ContentItemEntity>
{
    public void Configure(EntityTypeBuilder<ContentItemEntity> builder)
    {
        builder.ToTable("ContentItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Value)
            .HasColumnType("REAL"); 

        // Составной индекс
        builder.HasIndex(i => new { i.Key, i.Value });
    }
}