using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database.Configs;

public sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSourceEntity>
{
    private JsonSerializerOptions jsonSerializerOptions = new();
    
    public void Configure(EntityTypeBuilder<DataSourceEntity> builder)
    {
        throw new NotImplementedException();
        builder.ToTable("DataSources");
        
        builder.HasKey(x => x.Id);
        
        // builder.Property(t => t.FieldList).HasConversion(
        //     d => JsonSerializer.Serialize<>(d, jsonSerializerOptions),
        //     j => JsonSerializer.Deserialize<FieldList>(j, jsonSerializerOptions)!
        // );
        
        builder.Property(ds => ds.FieldList).HasColumnType("jsonb");

    }
}