using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReData.Database.Entities;

namespace ReData.Database.EntityConfiguration;

public class DataSetConfig : IEntityTypeConfiguration<DataSet>
{
    public void Configure(EntityTypeBuilder<DataSet> builder)
    {
        builder.ToTable("DataSets");

        builder.Property(d => d.Fields).HasColumnType("jsonb");
    }
}