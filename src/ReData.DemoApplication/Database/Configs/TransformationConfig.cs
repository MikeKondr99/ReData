using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;
using ReData.DemoApplication;

namespace ReData.Database.EntityConfiguration;

public sealed class TransformationConfig : IEntityTypeConfiguration<TransformationEntity>
{
    
    public void Configure(EntityTypeBuilder<TransformationEntity> builder)
    {
        builder.HasKey(x => new { x.DataSetId, x.Order });

        builder
            .HasOne<DataSetEntity>()
            .WithMany(ds => ds.Transformations)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.ToTable("Transformations");

        builder.Property(t => t.Data).HasConversion(
            d => JsonSerializer.Serialize<ITransformation>(d,new JsonSerializerOptions()),
            j => JsonSerializer.Deserialize<ITransformation>(j,new JsonSerializerOptions())!
        );

        builder.Property(t => t.Data).HasColumnType("jsonb");
    }
}