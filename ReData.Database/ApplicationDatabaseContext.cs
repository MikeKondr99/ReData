using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReData.Database.Entities;
using ReData.Database.EntityConfiguration;

namespace ReData.Database;

public class ApplicationDatabaseContext : DbContext, IDatabase
{
    public virtual DbSet<DataSource> DataSources => Set<DataSource>();

    public virtual DbSet<DataSourceParameter> DataSourceParameters => Set<DataSourceParameter>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("User ID=postgres;Password=postgres;Host=localhost;Port=5433;Database=ReData");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DataSourceConfig());
        modelBuilder.ApplyConfiguration(new DataSourceParameterConfig());
    }
}

public interface IDatabase
{
    public DbSet<T> Set<T>() where T : class;

    public Task<int> SaveChangesAsync(CancellationToken ct);
}