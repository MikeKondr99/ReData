using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApp.Database.Entities;

namespace ReData.DemoApp.Database.Configs;

public sealed class DataSetConfiguration : IEntityTypeConfiguration<DataSetEntity>
{
    public void Configure(EntityTypeBuilder<DataSetEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.ToTable("DataSets");
    }
}