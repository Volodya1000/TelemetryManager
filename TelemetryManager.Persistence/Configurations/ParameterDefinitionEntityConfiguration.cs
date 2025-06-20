using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Configurations;

public class ParameterDefinitionConfiguration
     : IEntityTypeConfiguration<ParameterDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<ParameterDefinitionEntity> builder)
    {
        builder.HasKey(p => new { p.TypeId, p.Name });

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.Unit).IsRequired();
        builder.Property(p => p.DataTypeName).IsRequired();

        builder.HasOne(pd => pd.ContentDefinition)
          .WithMany(cd => cd.Parameters)
          .HasForeignKey(pd => pd.TypeId) 
          .HasPrincipalKey(cd => cd.TypeId) 
          .OnDelete(DeleteBehavior.Restrict);
    }
}