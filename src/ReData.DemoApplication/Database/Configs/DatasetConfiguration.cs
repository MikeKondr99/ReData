using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database.Configs;

public sealed class DataSetConfiguration : IEntityTypeConfiguration<DataSetEntity>
{
    public void Configure(EntityTypeBuilder<DataSetEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.ToTable("DataSets");
    }
}