using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReData.Infrastructure.Entities;

namespace ReData.Infrastructure;

public class MainDatabaseContext : DbContext
{
    public DbSet<DataSource> DataSources => Set<DataSource>();

    // public DbSet<PostgresDataSource> PostgresDataSources => Set<PostgresDataSource>();
    //
    // public DbSet<CsvDataSource> CsvDataSources => Set<CsvDataSource>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=ReData");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataSource>()
            .HasDiscriminator(x => x.Type)
            .HasValue<DataSource>(DataSourceType.Unknown)
            .HasValue<PostgresDataSource>(DataSourceType.PostgreSql)
            .HasValue<CsvDataSource>(DataSourceType.Csv);

        modelBuilder.Entity<DataSource>()
            .UseTphMappingStrategy();

        modelBuilder.Entity<PostgresDataSource>()
            .OwnsOne(c => c.Options, d =>
            {
                d.ToJson("PostgresOptions");
            });
        modelBuilder.Entity<CsvDataSource>()
            .OwnsOne(c => c.Options, d =>
            {
                d.ToJson("CsvOptions");
            });
    }
}