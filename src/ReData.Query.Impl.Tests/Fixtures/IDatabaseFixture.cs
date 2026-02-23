using System.Data.Common;
using ReData.Query.Executors;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryExecutor> GetRunnerAsync();
    
    public DatabaseType GetDatabaseType();

    public DbConnection GetConnection();
}

