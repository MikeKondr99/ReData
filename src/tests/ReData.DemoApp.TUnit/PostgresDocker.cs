using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ReData.DemoApp.TUnit;

public sealed class PostgresDocker : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithName("ReData-TUnit")
        .WithImage("postgres:15-alpine")
        .WithDatabase("postgres")  // базовая БД, из неё будем CREATE DATABASE ...
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
        => await Container.StartAsync();

    public async ValueTask DisposeAsync()
        => await Container.DisposeAsync();
}
