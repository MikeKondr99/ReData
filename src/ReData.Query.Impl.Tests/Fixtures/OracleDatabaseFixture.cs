using Oracle.ManagedDataAccess.Client;
using ReData.Query.Runners;
using Testcontainers.Oracle;

namespace ReData.Query.Impl.Tests.Fixtures;

public class OracleDatabaseFixture : IDatabaseFixture
{
    private Factory _factory = new Factory();
    private OracleContainer Container { get; set; } = null!;
    private OracleConnection Connection { get; set; } = null!;
    private string ConnectionString { get; set; } = null!;
    
    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    
    public async Task<IQueryRunner> GetRunnerAsync()
    {
        return Runner ??= _factory.CreateQueryRunner(DatabaseType.Oracle, ConnectionString);
    }

    public DatabaseType GetDatabaseType()
    {
        return DatabaseType.Oracle;
    }


    public async Task InitializeAsync()
    {
        Container = new OracleBuilder().WithName("Oracle-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new OracleConnection(ConnectionString);
        await Connection.OpenAsync();
        
        await using var command = new OracleCommand(TestTableCreate, Connection);
        await command.ExecuteNonQueryAsync();

        foreach (var cmd in TestTableFill)
        {
            await using var command2 = new OracleCommand(cmd, Connection);
            await command2.ExecuteNonQueryAsync();
        }
        
    }
    
    public async Task DisposeAsync()
    {
        await Runner.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }

    private string TestTableCreate = """
                                  CREATE TABLE "Users" (
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
    private string[] TestTableFill = [
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (1, 'John', 'Doe', 30, 50000.50, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2020-05-10', 'YYYY-MM-DD'), TO_DATE('2023-10-01', 'YYYY-MM-DD'), 'Regular user')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (2, 'Jane', 'Smith', 25, 60000.00, TO_DATE('1995-07-22', 'YYYY-MM-DD'), TO_DATE('2021-03-15', 'YYYY-MM-DD'), TO_DATE('2023-09-28', 'YYYY-MM-DD'), 'Active user')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (3, 'John', 'Doe', 30, 55000.75, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2022-01-20', 'YYYY-MM-DD'), TO_DATE('2023-10-02', 'YYYY-MM-DD'), 'Promoted user')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (4, 'Alice', 'Johnson', 40, 75000.00, TO_DATE('1980-11-30', 'YYYY-MM-DD'), TO_DATE('2019-11-01', 'YYYY-MM-DD'), TO_DATE('2023-09-30', 'YYYY-MM-DD'), 'Manager')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (5, 'Jane', 'Smith', 25, 62000.50, TO_DATE('1995-07-22', 'YYYY-MM-DD'), TO_DATE('2021-03-15', 'YYYY-MM-DD'), TO_DATE('2023-10-03', 'YYYY-MM-DD'), 'Active user')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (6, 'Bob', 'Brown', 35, 45000.00, TO_DATE('1985-05-10', 'YYYY-MM-DD'), TO_DATE('2020-06-01', 'YYYY-MM-DD'), TO_DATE('2023-09-25', 'YYYY-MM-DD'), 'New user')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (7, 'Alice', 'Johnson', 40, 80000.00, TO_DATE('1980-11-30', 'YYYY-MM-DD'), TO_DATE('2019-11-01', 'YYYY-MM-DD'), TO_DATE('2023-10-04', 'YYYY-MM-DD'), 'Senior Manager')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (8, 'Mike', 'Davis', 28, 48000.00, TO_DATE('1993-02-14', 'YYYY-MM-DD'), TO_DATE('2021-07-15', 'YYYY-MM-DD'), TO_DATE('2023-09-29', 'YYYY-MM-DD'), 'Junior Developer')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (9, 'Sarah', 'Wilson', 32, 70000.00, TO_DATE('1989-08-20', 'YYYY-MM-DD'), TO_DATE('2018-12-01', 'YYYY-MM-DD'), TO_DATE('2023-10-05', 'YYYY-MM-DD'), 'Team Lead')""",
    """INSERT INTO "Users" ("UserId", "FirstName", "LastName", "Age", "Salary", "DateOfBirth", "JoinDate", "LastLoginDate", "Notes") VALUES (10, 'John', 'Doe', 30, 60000.00, TO_DATE('1990-01-15', 'YYYY-MM-DD'), TO_DATE('2020-05-10', 'YYYY-MM-DD'), TO_DATE('2023-10-06', 'YYYY-MM-DD'), 'Regular user')"""
    ];
    

}