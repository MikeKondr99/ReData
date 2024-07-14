using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;

namespace ReData.Database.EntityConfiguration;

public class DataSourceParameterConfig : IEntityTypeConfiguration<DataSourceParameter>
{
    public void Configure(EntityTypeBuilder<DataSourceParameter> builder)
    {
        builder.HasKey(x => new { x.DataSourceId, x.Key });

        builder
            .HasOne<DataSource>()
            .WithMany(ds => ds.Parameters)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.ToTable("DataSourceParameter");
    }
}