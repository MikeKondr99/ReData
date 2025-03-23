using Oracle.ManagedDataAccess.Client;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using Testcontainers.Oracle;

namespace ReData.Query.Impl.Tests.Fixtures;

public class OracleDatabaseFixture : IDatabaseFixture
{
    private QueryServicesFactory _factory = new QueryServicesFactory();
    private OracleContainer Container { get; set; } = null!;
    private OracleConnection Connection { get; set; } = null!;
    private string ConnectionString { get; set; } = null!;
    
    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    
    public async Task<IQueryRunner> GetRunnerAsync()
    {
        return Runner ??= _factory.CreateQueryRunner(DatabaseType.Oracle, ConnectionString);
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
                                  CREATE TABLE "TestTable" (
                                      "id" INTEGER,
                                      "Name" VARCHAR2(100),
                                      "MaxScore" NUMBER
                                  )
                                  """;
    
    private string[] TestTableFill = [
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (1,'George', 22.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (2,'Tom', 17.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (3,'Tim', 18.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (4,'Harry',21.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (5,'Ben',15.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (6,'Bob',18.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (7,'Phoebe',21.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (8,'Max', 14.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (9,'Lary', 17.0)",
                                  "INSERT INTO \"TestTable\" (\"id\", \"Name\", \"MaxScore\") VALUES (10,'Zach', 15.0)",
                                  ];

}