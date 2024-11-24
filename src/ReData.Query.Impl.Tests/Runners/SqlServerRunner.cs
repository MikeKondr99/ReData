using Microsoft.Data.SqlClient;
using ReData.Query.Impl.QueryBuilders;
using Testcontainers.MsSql;

namespace ReData.Query.Impl.Tests;

public class SqlServerRunner : ISqlRunner
{
    private MsSqlContainer Container { get; set; } = null!;
    private SqlConnection Connection { get; set; } = null!;

    public IQueryBuilder QueryBuilder { get; } = new QueryBuilderFactory().Create(DatabaseType.SqlServer);
    
    public bool Started { get; private set; }

    public async Task InitializeAsync()
    {
        Started = true;
        Container = new MsSqlBuilder().WithName("SqlServer-Tests").Build();
        await Container.StartAsync();
        Connection = new SqlConnection(Container.GetConnectionString());
        await Connection.OpenAsync();
        
        await using var command = new SqlCommand(TestTableSql, Connection);
        await command.ExecuteNonQueryAsync();
    }

    private string TestTableSql = """
                                  CREATE TABLE [TestTable] (
                                      "id" INT,
                                      "Name" VARCHAR(20),
                                      "MaxScore" DECIMAL
                                  );
                                  
                                  INSERT INTO [TestTable] ("id", "Name", "MaxScore") VALUES 
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
    
    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }

    public async Task<object?> Scalar(string sql)
    {
        await using var command = new SqlCommand(sql, Connection);
        var result = await command.ExecuteScalarAsync();
        return result;
    }

    public async Task<List<Player>> QueryAsync(string sql)
    {
        await using var command = new SqlCommand(sql, Connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();
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