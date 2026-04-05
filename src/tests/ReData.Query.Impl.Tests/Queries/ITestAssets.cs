using ReData.Query.Core;

namespace ReData.Query.Impl.Tests.Queries;

public interface ITestAssets : IAsyncLifetime
{

    public QueryBuilder CreateUsersQuery(IConstantRuntime? contantRuntime = null);
    
    public DatabaseType DatabaseType { get;  }
    
    public dynamic[] UsersDynamicArray { get; }
        
}
