using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Configurations;

public class ParameterDefinitionEntityConfiguration : IEntityTypeConfiguration<ParameterDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<ParameterDefinitionEntity> builder)
    {
        builder.HasKey(pd => pd.Id);
        builder.Property(pd => pd.Id).ValueGeneratedOnAdd();
    }
}