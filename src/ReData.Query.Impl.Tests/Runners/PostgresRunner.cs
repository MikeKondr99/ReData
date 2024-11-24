using Npgsql;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Impl.QueryBuilders;
using Testcontainers.PostgreSql;

namespace ReData.Query.Impl.Tests;

public class PostgresRunner : ISqlRunner
{
    private PostgreSqlContainer Container { get; set; } = null!;
    private NpgsqlConnection Connection { get; set; } = null!;
    public IQueryBuilder QueryBuilder { get; } = new QueryBuilderFactory().Create(DatabaseType.PostgreSql);
    
    public bool Started { get; private set; }
    
    #region sql
    private string TestTableSql = """
                                  CREATE TABLE "TestTable" (
                                      "id" BIGINT,
                                      "Name" VARCHAR(20),
                                      "MaxScore" DECIMAL
                                  );
                                  
                                  INSERT INTO "TestTable" ("id", "Name", "MaxScore") VALUES 
                                  (1,'George', 22.0),
                                  (2,'Tom', 17.0),
                                  (3,'Tim', 18.0),
                                  (4,'Harry',21.0),
                                  (5,'Ben',15.0),
                                  (6,'Bob',18.0),
                                  (7,'Phoebe',21.0),
                                  (8,'Max', 14.0),
                                  (9,'Lary', 17.0),
                                  (10,'Zach', 15.0)
                                  ;
                                  """;
    #endregion

    public async Task InitializeAsync()
    {
        Started = true;
        Container = new PostgreSqlBuilder().WithName("Postgres-Tests").Build();
        await Container.StartAsync();
        Connection = new NpgsqlConnection(Container.GetConnectionString());
        await Connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(TestTableSql, Connection);
        await command.ExecuteNonQueryAsync();
    }


    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }

    public async Task<object?> Scalar(string sql)
    {
        await using var command = new NpgsqlCommand(sql, Connection);
        var result = await command.ExecuteScalarAsync();
        return result;
    }


    public async Task<List<Player>> QueryAsync(string sql)
    {
        await using var command = new NpgsqlCommand(sql, Connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
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