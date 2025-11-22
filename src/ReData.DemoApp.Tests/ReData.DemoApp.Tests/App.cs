using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ReData.DemoApp.Database;
using ReData.DemoApp.Extensions;
using Testcontainers.PostgreSql;
using TickerQ.EntityFrameworkCore.DbContextFactory;

namespace ReData.DemoApp.Tests;

public class App : AppFixture<ReData.DemoApp.Services.DwhService>
{
    private static readonly PostgreSqlContainer PostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    protected override async Task PreSetupAsync()
    {
        // Start the container once before any tests run
        await PostgresContainer.StartAsync();
        // Create the three databases
        await CreateDatabases();
    }


    protected override Task SetupAsync()
    {
        return Task.CompletedTask;
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        Environment.SetEnvironmentVariable("ConnectinoStrings__DwhWrite", GetDwhConnectionString());
        Environment.SetEnvironmentVariable("ConnectinoStrings__DwhRead", GetDwhConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__ReData", GetReDataConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__TickerQ", GetTickerQConnectionString());
        // НЕ РАБОТАЕТ ХЗ ПОЧЕМУ
        // a.ConfigureAppConfiguration((context, config) =>
        // {
        //     config.AddJsonFile("appsettings.Test.json", optional: false);
        // });
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
        // do test service registration here
    }

    protected override Task TearDownAsync()
    {
        // do cleanups here
        return Task.CompletedTask;
    }


    private static async Task CreateDatabases()
    {
        var connectionString = PostgresContainer.GetConnectionString();

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create the three databases
        var databases = new[]
        {
            "dwh", "redata", "tickerq"
        };

        foreach (var dbName in databases)
        {
            var createDbCommand = new NpgsqlCommand(
                $"CREATE DATABASE {dbName}", connection);
            await createDbCommand.ExecuteNonQueryAsync();
        }
    }

    private static string GetDwhConnectionString()
    {
        return PostgresContainer.GetConnectionString().Replace("test_db", "dwh");
    }

    private static string GetReDataConnectionString()
    {
        return PostgresContainer.GetConnectionString().Replace("test_db", "redata");
    }

    private static string GetTickerQConnectionString()
    {
        return PostgresContainer.GetConnectionString().Replace("test_db", "tickerq");
    }
}