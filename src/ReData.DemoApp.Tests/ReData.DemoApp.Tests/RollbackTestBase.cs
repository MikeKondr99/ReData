using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using ReData.DemoApp.Database;

namespace ReData.DemoApp.Tests;

public class RollbackTestBase<TApp>(App App) : TestBase<TApp>, IAsyncLifetime
    where TApp : BaseFixture
{
    protected ApplicationDatabaseContext Db { get; private set; } = null!;

    private IDbContextTransaction Transaction { get; set; } = null!;

    private AsyncServiceScope serviceScope;

    public async Task InitializeAsync()
    {
        serviceScope = App.Services.CreateAsyncScope();
        Db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        Transaction = await Db.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await Db.DisposeAsync();
    }
}