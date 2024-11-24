namespace ReData.Query.Impl.Tests;

public interface ISqlRunner : IAsyncLifetime
{
    Task<object?> Scalar(string sql);

    Task<List<Player>> QueryAsync(string sql);
    
    IQueryBuilder QueryBuilder { get; }

    public bool Started { get; }
}

public record struct Player
{
    public int id;
    public string Name;
    public decimal MaxScore;
}


[CollectionDefinition("Runners")]
public class RunnersCollection : ICollectionFixture<ISqlRunner>;