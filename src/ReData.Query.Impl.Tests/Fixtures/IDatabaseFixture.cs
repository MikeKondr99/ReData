using System.Data.Common;
using ReData.Query.Impl.Tests.Queries;
using ReData.Query.Runners;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryRunner> GetRunnerAsync();
    
    public DatabaseType GetDatabaseType();

    public DbConnection GetConnection();
}

