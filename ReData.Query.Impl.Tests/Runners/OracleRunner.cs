using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Impl.QueryBuilders;
using Testcontainers.Oracle;

namespace ReData.Query.Impl.Tests.Runners;

public class OracleRunner : ISqlRunner
{
    private OracleContainer Container { get; set; }
    private OracleConnection Connection { get; set; } = null!;

    public IQueryBuilder QueryBuilder { get; } = new QueryBuilderFactory().Create(DatabaseType.Oracle);

    public bool Started { get; private set; }
    
    public async Task InitializeAsync()
    {
        Started = true;
        Container = new OracleBuilder().WithName("Oracle-Tests").Build();
        await Container.StartAsync();
        Connection = new OracleConnection(Container.GetConnectionString());
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
        await Connection.DisposeAsync();
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


    public async Task<object?> Scalar(string sql)
    {
        await using var command = new OracleCommand(sql, Connection);
        var result = await command.ExecuteScalarAsync();
        return result;
    }


    public async Task<List<Player>> QueryAsync(string sql)
    {
        await using var command = new OracleCommand(sql, Connection);
        await using DbDataReader reader = await command.ExecuteReaderAsync();
        List<Player> result = new List<Player>();
        while (await reader.ReadAsync())
        {
            result.Add(new Player()
            {
                id = reader.GetInt32(0),
                Name = reader.GetString(1),
                MaxScore = reader.GetDecimal(2),
            });
        }
        return result;
    }
}