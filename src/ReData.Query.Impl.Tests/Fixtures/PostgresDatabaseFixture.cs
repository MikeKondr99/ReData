using System.Data.Common;
using Npgsql;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Executors;
using Testcontainers.PostgreSql;

namespace ReData.Query.Impl.Tests.Fixtures;

public class PostgresDatabaseFixture : IDatabaseFixture
{
    private PostgreSqlContainer Container { get; set; } = null!;
    private NpgsqlConnection Connection { get; set; } = null!;
    private string ConnectionString { get; set; } = null!;

    private IQueryExecutor? runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам

    public Task<IQueryExecutor> GetRunnerAsync()
    {
        return Task.FromResult(runner ??= Factory.CreateQueryExecuter(DatabaseType.PostgreSql));
    }

    public DatabaseType GetDatabaseType()
    {
        return DatabaseType.PostgreSql;
    }

    public DbConnection GetConnection()
    {
        return Connection;
    }

    private string testTableSql = """
                                  CREATE TABLE "User" (
                                      "UserId" SERIAL PRIMARY KEY,
                                      "FirstName" TEXT,
                                      "LastName" TEXT,
                                      "Age" INT,
                                      "Salary" DOUBLE PRECISION,
                                      "DateOfBirth" DATE,
                                      "JoinDate" DATE,
                                      "LastLoginDate" DATE,
                                      "Notes" TEXT
                                  );

                                  INSERT INTO "User" ("FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes")
                                  VALUES
                                      ('John', 'Doe', 30, 50000.50, '1990-01-15', '2020-05-10', '2023-10-01', 'Regular user'),
                                      ('Jane', 'Smith', 25, 60000.00, '1995-07-22', '2021-03-15', '2023-09-28', 'Active user'),
                                      ('John', 'Doe', 30, 55000.75, '1990-01-15', '2022-01-20', '2023-10-02', 'Promoted user'),
                                      ('Alice', 'Johnson', 40, 75000.00, '1980-11-30', '2019-11-01', '2023-09-30', 'Manager'),
                                      ('Jane', 'Smith', 25, 62000.50, '1995-07-22', '2021-03-15', '2023-10-03', 'Active user'),
                                      ('Bob', 'Brown', 35, 45000.00, '1985-05-10', '2020-06-01', '2023-09-25', 'New user'),
                                      ('Alice', 'Johnson', 40, 80000.00, '1980-11-30', '2019-11-01', '2023-10-04', 'Senior Manager'),
                                      ('Mike', 'Davis', 28, 48000.00, '1993-02-14', '2021-07-15', '2023-09-29', 'Junior Developer'),
                                      ('Sarah', 'Wilson', 32, 70000.00, '1989-08-20', '2018-12-01', '2023-10-05', 'Team Lead'),
                                      ('John', 'Doe', 30, 60000.00, '1990-01-15', '2020-05-10', '2023-10-06', 'Regular user'),
                                      ('Emily', 'Clark', 27, 52000.00, '1994-03-25', '2022-02-10', '2023-09-27', 'Intern'),
                                      ('Jane', 'Smith', 25, 65000.00, '1995-07-22', '2021-03-15', '2023-10-07', 'Active user'),
                                      ('Chris', 'Evans', 38, 90000.00, '1983-09-12', '2017-10-01', '2023-10-08', 'Director'),
                                      ('Alice', 'Johnson', 40, 85000.00, '1980-11-30', '2019-11-01', '2023-10-09', 'Senior Manager'),
                                      ('Bob', 'Brown', 35, 47000.00, '1985-05-10', '2020-06-01', '2023-10-10', 'New user');
                                  """;

    public async Task InitializeAsync()
    {
        Container = new PostgreSqlBuilder().WithName("Postgres-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new NpgsqlConnection(ConnectionString);
        await Connection.OpenAsync();

        await using var command = new NpgsqlCommand(testTableSql, Connection);
        await command.ExecuteNonQueryAsync();
    }


    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();

        FunctionStorage? functions = Factory.CreateFunctionStorage(DatabaseType.PostgreSql) as FunctionStorage;
        var usage = functions.FunctionUsage;
        GenerateMarkdownReport(usage);
    }

    private static void GenerateMarkdownReport(IReadOnlyDictionary<string, int> usage)
    {
        // Now usage contains ALL functions, with 0 values for unused ones

        // Get functions by usage count
        var unusedFunctions = usage
            .Where(kvp => kvp.Value == 0)
            .Select(kvp => kvp.Key)
            .OrderBy(f => f)
            .ToList();

        var lowUsageFunctions = usage
            .Where(kvp => kvp.Value > 0 && kvp.Value <= 1)
            .OrderBy(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .ToList();

        var wellTestedFunctions = usage
            .Where(kvp => kvp.Value > 1)
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .ToList();

        var totalFunctions = usage.Count;
        var testedFunctions = usage.Count(kvp => kvp.Value > 0);
        var coveragePercent = totalFunctions > 0 ? (testedFunctions * 100 / totalFunctions) : 0;
        var wellTestedCount = wellTestedFunctions.Count;
        var qualityPercent = totalFunctions > 0 ? (wellTestedCount * 100 / totalFunctions) : 0;

        var markdown = $"""
                        # Function Test Coverage Report

                        Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

                        ## 📊 Summary

                        | Metric | Count | Percentage |
                        |--------|-------|------------|
                        | Total Functions | {totalFunctions} | 100% |
                        | Tested Functions | {testedFunctions} | {coveragePercent}% |
                        | Well-Tested (>2 calls) | {wellTestedCount} | {qualityPercent}% |

                        ## ❌ Untested Functions ({unusedFunctions.Count})

                        These functions were never called during testing:

                        {string.Join("\n", unusedFunctions.Select(f => $"* `{f}`"))}

                        ## ⚠️ Low Usage Functions ({lowUsageFunctions.Count})

                        These functions were called 1 time and may need more test coverage:

                        {string.Join("\n", lowUsageFunctions.Select(kvp => $"* `{kvp.Key}` ({kvp.Value} call{(kvp.Value != 1 ? "s" : "")})"))}

                        ## ✅ Well-Tested Functions ({wellTestedCount})

                        These functions were called more than 2 times:

                        {string.Join("\n", wellTestedFunctions.Take(50).Select(kvp => $"* `{kvp.Key}` ({kvp.Value} calls)"))}
                        {(wellTestedFunctions.Count > 50 ? $"\n*... and {wellTestedFunctions.Count - 50} more well-tested functions*" : "")}

                        ---
                        *Report generated by test coverage analyzer*
                        """;

        var reportPath = "function-coverage-report.md";
        File.WriteAllText(reportPath, markdown);
        Console.WriteLine($"📊 Markdown report generated: {Path.GetFullPath(reportPath)}");

        // Also output quick summary to console
    }
}