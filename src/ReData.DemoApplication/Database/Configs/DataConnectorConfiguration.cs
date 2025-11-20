using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database.Configs;

public sealed class DataConnectorConfiguration : IEntityTypeConfiguration<DataConnectorEntity>
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new();

    public void Configure(EntityTypeBuilder<DataConnectorEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(t => t.FieldList).HasConversion(
            d => JsonSerializer.Serialize(d, jsonSerializerOptions),
            j => JsonSerializer.Deserialize<IReadOnlyList<Field>>(j, jsonSerializerOptions)!
        );

        builder.Property(ds => ds.FieldList).HasColumnType("jsonb");

        builder.ToTable("DataConnectors");
    }
}