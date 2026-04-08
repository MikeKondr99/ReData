using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReData.DemoApp.Services;
using ReData.DemoApp.Tests.Init;
using TUnit.Core.Interfaces;

namespace ReData.DemoApp.Tests;

public sealed class DefaultReDataApp : WebApplicationFactory<Program>, IAsyncInitializer, IAsyncDisposable
{
    [ClassDataSource<DefaultDatabaseFixture>(Shared = SharedType.PerTestSession)]
    public required DefaultDatabaseFixture Db { get; init; } = null!;
    
    public InitData Data { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // 1) ВАЖНО: это форсирует построение хоста/пайплайна
        // и инициализацию FastEndpoints.
        Client = CreateClient();

        // 2) Теперь можно безопасно дергать команды FE
        Data = await InitData.CreateAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test"); 
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DwhWrite"] = Db.DwhConnectionString,
                ["ConnectionStrings:DwhRead"]  = Db.DwhConnectionString,
                ["ConnectionStrings:ReData"]   = Db.ReDataConnectionString,
                ["ConnectionStrings:TickerQ"]  = Db.TickerQConnectionString,
            });
        });

        builder.ConfigureServices(services =>
        {
            // если нужно — регистрация тестовых сервисов
        });
    }
}
