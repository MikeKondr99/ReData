using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;

namespace ReData.Database.EntityConfiguration;

public sealed class DataSourceConfig : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.ToTable("DataSources");
    }
}