using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database.Configs;

public sealed class DataSetConfiguration : IEntityTypeConfiguration<DataSetEntity>
{
    private JsonSerializerOptions jsonSerializerOptions = new();
    public void Configure(EntityTypeBuilder<DataSetEntity> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        builder.ToTable("DataSets");
        
        builder.Property(t => t.FieldList).HasConversion(
            d => JsonSerializer.Serialize<List<Field>>(d, jsonSerializerOptions),
            j => JsonSerializer.Deserialize<List<Field>>(j, jsonSerializerOptions)!
        );
        
        builder.Property(ds => ds.FieldList).HasColumnType("jsonb");
    }
}