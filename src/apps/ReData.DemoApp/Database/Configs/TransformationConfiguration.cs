using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Transformations;
using StrictId.EFCore.ValueConverters;

namespace ReData.DemoApp.Database.Configs;

public sealed class TransformationConfiguration : IEntityTypeConfiguration<TransformationEntity>
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new();

    public void Configure(EntityTypeBuilder<TransformationEntity> builder)
    {
        builder.Property(x => x.DataSetId).HasConversion<IdTypedToGuidConverter<DatasetEntity>>();

        builder.HasKey(x => new
        {
            x.DataSetId,
            x.Order
        });

        builder
            .HasOne<DatasetEntity>()
            .WithMany(ds => ds.Transformations)
            .HasForeignKey(ds => ds.DataSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Transformations");

        builder.Property(t => t.Data).HasConversion(
            d => JsonSerializer.Serialize<Transformation>(d, jsonSerializerOptions),
            j => JsonSerializer.Deserialize<Transformation>(j, jsonSerializerOptions)!
        );

        builder.Property(t => t.Data).HasColumnType("jsonb");
    }
}
