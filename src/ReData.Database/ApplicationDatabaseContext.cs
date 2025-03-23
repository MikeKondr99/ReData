using Microsoft.EntityFrameworkCore;
using ReData.Database.EntityConfiguration;

namespace ReData.Database;

public sealed class ApplicationDatabaseContext : DbContext, IDatabase 
{

    public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DataSourceConfig());
        modelBuilder.ApplyConfiguration(new DataSourceParameterConfig());
        modelBuilder.ApplyConfiguration(new DataSetConfig());
        modelBuilder.ApplyConfiguration(new TransformationConfig());
    }
}

public interface IDatabase
{
    public DbSet<T> Set<T>() where T : class;

    public Task<int> SaveChangesAsync(CancellationToken ct);
}