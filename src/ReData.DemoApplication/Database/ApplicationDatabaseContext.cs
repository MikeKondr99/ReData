using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database.Configs;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database;

public sealed class ApplicationDatabaseContext : DbContext
{

    public DbSet<DataSetEntity> DataSets => Set<DataSetEntity>();

    public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DataSetConfiguration());
        modelBuilder.ApplyConfiguration(new TransformationConfiguration());
    }
}