using Npgsql;
using TUnit.Core.Interfaces;

namespace ReData.DemoApp.TUnit;

public abstract class BaseDatabaseFixture : IAsyncInitializer, IAsyncDisposable
{
    [ClassDataSource<PostgresDocker>(Shared = SharedType.PerTestSession)]
    public required PostgresDocker Docker { get; init; } = null!;

    public string DwhConnectionString { get; private set; } = null!;
    public string ReDataConnectionString { get; private set; } = null!;
    public string TickerQConnectionString { get; private set; } = null!;

    // Имена БД — чтобы были стабильны внутри фикстуры
    protected string DwhDbName { get; private set; } = null!;
    protected string ReDataDbName { get; private set; } = null!;
    protected string TickerQDbName { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // чтобы несколько наборов могли жить рядом (или параллельно) без конфликтов
        var suffix = Guid.NewGuid().ToString("N")[..8];

        DwhDbName = $"dwh_{suffix}";
        ReDataDbName = $"redata_{suffix}";
        TickerQDbName = $"tickerq_{suffix}";

        await CreateDatabaseAsync(DwhDbName);
        await CreateDatabaseAsync(ReDataDbName);
        await CreateDatabaseAsync(TickerQDbName);

        DwhConnectionString = WithDatabase(Docker.Container.GetConnectionString(), DwhDbName);
        ReDataConnectionString = WithDatabase(Docker.Container.GetConnectionString(), ReDataDbName);
        TickerQConnectionString = WithDatabase(Docker.Container.GetConnectionString(), TickerQDbName);

        // Хук “заполнить базы кодом”
        await SeedAsync();
    }

    public virtual ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    protected virtual Task SeedAsync() => Task.CompletedTask;

    protected async Task CreateDatabaseAsync(string dbName)
    {
        // Подключаемся к "postgres" (или той базовой, что в Container.GetConnectionString())
        await using var conn = new NpgsqlConnection(Docker.Container.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand($@"CREATE DATABASE ""{dbName}"";", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string WithDatabase(string baseCs, string dbName)
    {
        var b = new NpgsqlConnectionStringBuilder(baseCs)
        {
            Database = dbName
        };
        return b.ConnectionString;
    }
}