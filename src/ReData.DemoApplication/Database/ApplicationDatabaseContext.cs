using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database.Configs;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Database;

// dotnet ef migrations add DataSources --project ./src/ReData.DemoApplication --context ApplicationDatabaseContext

public sealed class ApplicationDatabaseContext : DbContext
{
    public DbSet<DataSetEntity> DataSets => Set<DataSetEntity>();

    public DbSet<DataConnectorEntity> DataConnectors => Set<DataConnectorEntity>();

    public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DataSetConfiguration());
        modelBuilder.ApplyConfiguration(new DataConnectorConfiguration());
        modelBuilder.ApplyConfiguration(new TransformationConfiguration());
    }
}