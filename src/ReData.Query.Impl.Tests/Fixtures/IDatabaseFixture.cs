using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryRunner> GetRunnerAsync();
    
    public DatabaseType GetDatabaseType();
}

public record struct Player
{
    public int id;
    public string Name;
    public decimal MaxScore;
}


[CollectionDefinition("Runners")]
public class RunnersCollection : ICollectionFixture<IDatabaseFixture>;