using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database.Configs;
using ReData.DemoApp.Database.Entities;

namespace ReData.DemoApp.Database;

// dotnet ef migrations add DataSources --project ./src/ReData.DemoApp --context ApplicationDatabaseContext

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