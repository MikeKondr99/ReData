using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;

namespace ReData.Database.EntityConfiguration;

public sealed class TransformationConfig : IEntityTypeConfiguration<Transformation>
{
    
    public void Configure(EntityTypeBuilder<Transformation> builder)
    {
        builder.HasKey(x => new { x.DataSetId, x.Order });

        builder
            .HasOne<DataSet>()
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