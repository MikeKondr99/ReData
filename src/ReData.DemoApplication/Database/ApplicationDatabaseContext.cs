using Microsoft.EntityFrameworkCore;
using ReData.Database.Entities;
using ReData.Database.EntityConfiguration;

namespace ReData.Database;

public sealed class ApplicationDatabaseContext : DbContext
{

    public DbSet<DataSetEntity> DataSets => Set<DataSetEntity>();

    public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DataSetConfig());
        modelBuilder.ApplyConfiguration(new TransformationConfig());
    }
}
