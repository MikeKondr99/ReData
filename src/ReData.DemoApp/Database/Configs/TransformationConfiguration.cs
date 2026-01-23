using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Database.Configs;

public sealed class TransformationConfiguration : IEntityTypeConfiguration<TransformationEntity>
{
    private JsonSerializerOptions jsonSerializerOptions = new();

    public void Configure(EntityTypeBuilder<TransformationEntity> builder)
    {
        builder.HasKey(x => new
        {
            x.DataSetId,
            x.Order
        });

        builder
            .HasOne<DataSetEntity>()
            .WithMany(ds => ds.Transformations)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Transformations");

        builder.Property(t => t.Data).HasConversion(
            d => JsonSerializer.Serialize<Transformation>(d, jsonSerializerOptions),
            j => JsonSerializer.Deserialize<Transformation>(j, jsonSerializerOptions)!
        );

        builder.Property(t => t.Data).HasColumnType("jsonb");
    }
}