using ReData.Query.Core;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Queries;

public interface ITestAssets : IAsyncLifetime
{
    public QueryBuilder UsersQuery { get; }
    
    public DatabaseType DatabaseType { get;  }
    
    public IReadOnlyList<Dictionary<string,IValue>> UsersData { get; }
}