using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Tests.Queries;

public interface ITestAssets : IAsyncLifetime
{
    public QueryBuilder UsersQuery { get; }
    
    public DatabaseType DatabaseType { get;  }
    
    public IReadOnlyList<Dictionary<string,IValue>> UsersData { get; }
}