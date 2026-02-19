using ReData.Query.Core;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Queries;

public interface ITestAssets : IAsyncLifetime
{

    public QueryBuilder CreateUsersQuery(IVariableRuntime? variableRuntime = null);
    
    public DatabaseType DatabaseType { get;  }
    
    public dynamic[] UsersDynamicArray { get; }
        
}
