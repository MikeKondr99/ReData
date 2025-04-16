using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Tests.Queries;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryRunner> GetRunnerAsync();
    
    public DatabaseType GetDatabaseType();
}

[CollectionDefinition("Runners")]
public class RunnersCollection : ICollectionFixture<IDatabaseFixture>, ICollectionFixture<ITestAssets>;

