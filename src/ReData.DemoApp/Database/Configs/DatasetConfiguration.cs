using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApp.Database.Entities;
using StrictId.EFCore.ValueConverters;

namespace ReData.DemoApp.Database.Configs;

public sealed class DataSetConfiguration : IEntityTypeConfiguration<DataSetEntity>
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new();

    public void Configure(EntityTypeBuilder<DataSetEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(ds => ds.Id).HasConversion<IdTypedToGuidConverter<DataSetEntity>>();

        builder.Property(t => t.FieldList).HasConversion(
            d => JsonSerializer.Serialize(d, jsonSerializerOptions),
            j => JsonSerializer.Deserialize<IReadOnlyList<DataSetField>>(j, jsonSerializerOptions)!
        );

        builder.Property(ds => ds.FieldList).HasColumnType("jsonb");

        builder.ToTable("DataSets");
    }
}
