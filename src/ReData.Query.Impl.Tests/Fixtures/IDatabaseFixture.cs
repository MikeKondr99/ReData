using ReData.Query.Impl.Runners;

namespace ReData.Query.Impl.Tests.Fixtures;

public interface IDatabaseFixture : IAsyncLifetime
{
    public Task<IQueryRunner> GetRunnerAsync();
}

public record struct Player
{
    public int id;
    public string Name;
    public decimal MaxScore;
}


[CollectionDefinition("Runners")]
public class RunnersCollection : ICollectionFixture<IDatabaseFixture>;