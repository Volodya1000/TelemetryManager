using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Configurations;

public class ContentDefinitionEntityConfiguration : IEntityTypeConfiguration<ContentDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<ContentDefinitionEntity> builder)
    {
        builder.HasKey(cd => cd.TypeId);
        builder.Property(cd => cd.TypeId).ValueGeneratedNever();

        builder.HasMany(cd => cd.Parameters)
               .WithOne(pd => pd.ContentDefinition)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
