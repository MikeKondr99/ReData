using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database.Configs;
using ReData.DemoApp.Database.Entities;

namespace ReData.DemoApp.Database;

// dotnet ef migrations add <Name> --project ./src/libs/ReData.Database --startup-project ./src/apps/ReData.DemoApp --context ApplicationDatabaseContext --output-dir ./Migrations

public sealed class ApplicationDatabaseContext : DbContext
{
    public DbSet<DatasetEntity> DataSets => Set<DatasetEntity>();

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
