using System.Data.Common;
using ClickHouse.Client.ADO;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Executors;
using Testcontainers.Oracle;

namespace ReData.Query.Impl.Tests.Fixtures;

public class OracleDatabaseFixture : IDatabaseFixture
{
    private OracleContainer Container { get; set; } = null!;
    private OracleConnection Connection { get; set; } = null!;
    private string ConnectionString { get; set; } = null!;
    
    private IQueryExecutor? runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    
    public Task<IQueryExecutor> GetRunnerAsync()
    {
        return Task.FromResult(runner ??= Factory.CreateQueryExecuter(DatabaseType.Oracle));
    }

    public DatabaseType GetDatabaseType()
    {
        return DatabaseType.Oracle;
    }

    public DbConnection GetConnection()
    {
        return Connection;
    }

    public async Task InitializeAsync()
    {
        Container = new OracleBuilder().WithName("Oracle-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new OracleConnection(ConnectionString);
        await Connection.OpenAsync();
        
        await using var command = new OracleCommand(testTableCreate, Connection);
        await command.ExecuteNonQueryAsync();

        foreach (var cmd in testTableFill)
        {
            await using var command2 = new OracleCommand(cmd, Connection);
            await command2.ExecuteNonQueryAsync();
        }
        
    }
    
    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }

    private string testTableCreate = """
                                  CREATE TABLE "User" (
                                      "UserId" INTEGER,
                                      "FirstName" VARCHAR2(200),
                                      "LastName" VARCHAR2(200),
                                      "Age" NUMBER,
                                      "Salary" NUMBER,
                                      "DateOfBirth" DATE,
                                      "JoinDate" DATE,
                                      "LastLoginDate" DATE,
                                      "Notes" VARCHAR2(200)
                                  )
                                  """;
    private readonly string[] testTableFill = [
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (1, 'John', 'Doe', 30, 50000.50, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2020-05-10', 'YYYY-MM-DD'), TO_DATE('2023-10-01', 'YYYY-MM-DD'), 'Regular user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (2, 'Jane', 'Smith', 25, 60000.00, TO_DATE('1995-07-22', 'YYYY-MM-DD'), TO_DATE('2021-03-15', 'YYYY-MM-DD'), TO_DATE('2023-09-28', 'YYYY-MM-DD'), 'Active user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (3, 'John', 'Doe', 30, 55000.75, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2022-01-20', 'YYYY-MM-DD'), TO_DATE('2023-10-02', 'YYYY-MM-DD'), 'Promoted user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (4, 'Alice', 'Johnson', 40, 75000.00, TO_DATE('1980-11-30', 'YYYY-MM-DD'), TO_DATE('2019-11-01', 'YYYY-MM-DD'), TO_DATE('2023-09-30', 'YYYY-MM-DD'), 'Manager')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (5, 'Jane', 'Smith', 25, 62000.50, TO_DATE('1995-07-22', 'YYYY-MM-DD'), TO_DATE('2021-03-15', 'YYYY-MM-DD'), TO_DATE('2023-10-03', 'YYYY-MM-DD'), 'Active user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (6, 'Bob', 'Brown', 35, 45000.00, TO_DATE('1985-05-10', 'YYYY-MM-DD'), TO_DATE('2020-06-01', 'YYYY-MM-DD'), TO_DATE('2023-09-25', 'YYYY-MM-DD'), 'New user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (7, 'Alice', 'Johnson', 40, 80000.00, TO_DATE('1980-11-30', 'YYYY-MM-DD'), TO_DATE('2019-11-01', 'YYYY-MM-DD'), TO_DATE('2023-10-04', 'YYYY-MM-DD'), 'Senior Manager')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (8, 'Mike', 'Davis', 28, 48000.00, TO_DATE('1993-02-14', 'YYYY-MM-DD'), TO_DATE('2021-07-15', 'YYYY-MM-DD'), TO_DATE('2023-09-29', 'YYYY-MM-DD'), 'Junior Developer')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (9, 'Sarah', 'Wilson', 32, 70000.00, TO_DATE('1989-08-20', 'YYYY-MM-DD'), TO_DATE('2018-12-01', 'YYYY-MM-DD'), TO_DATE('2023-10-05', 'YYYY-MM-DD'), 'Team Lead')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (10, 'John', 'Doe', 30, 60000.00, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2020-05-10', 'YYYY-MM-DD'), TO_DATE('2023-10-06', 'YYYY-MM-DD'), 'Regular user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (11, 'Emily', 'Clark', 27, 52000.00, TO_DATE('1994-03-25','YYYY-MM-DD'), TO_DATE('2022-02-10','YYYY-MM-DD'), TO_DATE('2023-09-27','YYYY-MM-DD'), 'Intern')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (12, 'Jane', 'Smith', 25, 65000.00, TO_DATE('1995-07-22','YYYY-MM-DD'), TO_DATE('2021-03-15','YYYY-MM-DD'), TO_DATE('2023-10-07','YYYY-MM-DD'), 'Active user')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (13, 'Chris', 'Evans', 38, 90000.00, TO_DATE('1983-09-12','YYYY-MM-DD'), TO_DATE('2017-10-01','YYYY-MM-DD'), TO_DATE('2023-10-08','YYYY-MM-DD'), 'Director')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (14, 'Alice', 'Johnson', 40, 85000.00, TO_DATE('1980-11-30','YYYY-MM-DD'), TO_DATE('2019-11-01','YYYY-MM-DD'), TO_DATE('2023-10-09','YYYY-MM-DD'), 'Senior Manager')""",
    """INSERT INTO "User" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (15, 'Bob', 'Brown', 35, 47000.00, TO_DATE('1985-05-10','YYYY-MM-DD'), TO_DATE('2020-06-01','YYYY-MM-DD'), TO_DATE('2023-10-10','YYYY-MM-DD'), 'New user')""",
    ];
    

}