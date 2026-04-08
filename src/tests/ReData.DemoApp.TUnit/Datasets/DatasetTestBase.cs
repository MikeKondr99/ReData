using Microsoft.Extensions.DependencyInjection;
using ReData.DemoApp.Database;
using TUnit.Core;

namespace ReData.DemoApp.TUnit.Datasets;

public abstract class DatasetTestBase
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }

    protected ApplicationDatabaseContext Db { get; private set; } = null!;

    private AsyncServiceScope _scope;

    [Before(HookType.Test)]
    public void BeforeEachTest()
    {
        _scope = App.Services.CreateAsyncScope();
        Db = _scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
    }

    [After(HookType.Test)]
    public async Task AfterEachTest()
    {
        await Db.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
