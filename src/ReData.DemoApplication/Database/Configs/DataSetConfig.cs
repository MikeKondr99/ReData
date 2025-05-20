using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;

namespace ReData.Database.EntityConfiguration;

public sealed class DataSetConfig : IEntityTypeConfiguration<DataSetEntity>
{
    public void Configure(EntityTypeBuilder<DataSetEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.ToTable("DataSets");
    }
}