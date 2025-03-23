using Npgsql;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using Testcontainers.PostgreSql;

namespace ReData.Query.Impl.Tests.Fixtures;

public class PostgresDatabaseFixture : IDatabaseFixture
{
    private QueryServicesFactory _factory = new QueryServicesFactory();
    private PostgreSqlContainer Container { get; set; } = null!;
    private NpgsqlConnection Connection { get; set; } = null!;

    private string ConnectionString { get; set; } = null!;

    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    public async Task<IQueryRunner> GetRunnerAsync()
    {
        return Runner ??= _factory.CreateQueryRunner(DatabaseType.PostgreSql, ConnectionString);
    }
    
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
        Container = new PostgreSqlBuilder().WithName("Postgres-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new NpgsqlConnection(ConnectionString);
        await Connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(TestTableSql, Connection);
        await command.ExecuteNonQueryAsync();
    }


    public async Task DisposeAsync()
    {
        await Runner.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}