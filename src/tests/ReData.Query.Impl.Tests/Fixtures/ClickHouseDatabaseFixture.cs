using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using ReData.Query.Executors;
using Testcontainers.ClickHouse;

namespace ReData.Query.Impl.Tests.Fixtures;

public class ClickHouseDatabaseFixture : IDatabaseFixture
{
    private ClickHouseContainer Container { get; set; } = null!;
    private ClickHouseConnection Connection { get; set; } = null!;

    private string ConnectionString { get; set; } = null!;
    private IQueryExecutor? runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам

    public Task<IQueryExecutor> GetRunnerAsync()
    {
        return Task.FromResult(runner ??= Factory.CreateQueryExecuter(DatabaseType.ClickHouse));
    }

    public DatabaseType GetDatabaseType()
    {
        return DatabaseType.ClickHouse;
    }

    public DbConnection GetConnection()
    {
        return Connection;
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("Создаём ClickHouse");
        Container = new ClickHouseBuilder().WithName("ClickHouse-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new ClickHouseConnection(ConnectionString);
        await Connection.OpenAsync();
        await Connection.ExecuteStatementAsync(testTableCreate);
        await Connection.ExecuteStatementAsync(testTableFill);
    }

    private string testTableCreate = """
                                     CREATE TABLE "User" (
                                         "UserId" Int32,
                                         "FirstName" String,
                                         "LastName" String,
                                         "Age" Int32,
                                         "Salary" Float64,
                                         "DateOfBirth" Date,
                                         "JoinDate" Date,
                                         "LastLoginDate" Date,
                                         "Notes" String
                                     ) ENGINE = MergeTree()
                                     ORDER BY "UserId";
                                     """;

    private string testTableFill =
        """
        INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes")
        VALUES
            (1, 'John', 'Doe', 30, 50000.50, '1990-01-15', '2020-05-10', '2023-10-01', 'Regular user'),
            (2, 'Jane', 'Smith', 25, 60000.00, '1995-07-22', '2021-03-15', '2023-09-28', 'Active user'),
            (3, 'John', 'Doe', 30, 55000.75, '1990-01-15', '2022-01-20', '2023-10-02', 'Promoted user'),
            (4, 'Alice', 'Johnson', 40, 75000.00, '1980-11-30', '2019-11-01', '2023-09-30', 'Manager'),
            (5, 'Jane', 'Smith', 25, 62000.50, '1995-07-22', '2021-03-15', '2023-10-03', 'Active user'),
            (6, 'Bob', 'Brown', 35, 45000.00, '1985-05-10', '2020-06-01', '2023-09-25', 'New user'),
            (7, 'Alice', 'Johnson', 40, 80000.00, '1980-11-30', '2019-11-01', '2023-10-04', 'Senior Manager'),
            (8, 'Mike', 'Davis', 28, 48000.00, '1993-02-14', '2021-07-15', '2023-09-29', 'Junior Developer'),
            (9, 'Sarah', 'Wilson', 32, 70000.00, '1989-08-20', '2018-12-01', '2023-10-05', 'Team Lead'),
            (10, 'John', 'Doe', 30, 60000.00, '1990-01-15', '2020-05-10', '2023-10-06', 'Regular user'),
            (11, 'Emily', 'Clark', 27, 52000.00, '1994-03-25', '2022-02-10', '2023-09-27', 'Intern'),
            (12, 'Jane', 'Smith', 25, 65000.00, '1995-07-22', '2021-03-15', '2023-10-07', 'Active user'),
            (13, 'Chris', 'Evans', 38, 90000.00, '1983-09-12', '2017-10-01', '2023-10-08', 'Director'),
            (14, 'Alice', 'Johnson', 40, 85000.00, '1980-11-30', '2019-11-01', '2023-10-09', 'Senior Manager'),
            (15, 'Bob', 'Brown', 35, 47000.00, '1985-05-10', '2020-06-01', '2023-10-10', 'New user');
        """;

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}