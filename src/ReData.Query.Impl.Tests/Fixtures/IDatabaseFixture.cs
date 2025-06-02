using ReData.Query.Impl.Tests.Queries;
using ReData.Query.Runners;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryRunner> GetRunnerAsync();
    
    public DatabaseType GetDatabaseType();
}

[CollectionDefinition("Runners")]
#pragma warning disable CA1711
public class RunnersCollection : ICollectionFixture<IDatabaseFixture>, ICollectionFixture<ITestAssets>;
#pragma warning restore CA1711

