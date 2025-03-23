using MySql.Data.MySqlClient;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using Testcontainers.MySql;

namespace ReData.Query.Impl.Tests.Fixtures;

public class MySqlDatabaseFixture : IAsyncLifetime, IDatabaseFixture
{
    private QueryServicesFactory _factory = new QueryServicesFactory();
    private MySqlContainer Container { get; set; } = null!;
    private MySqlConnection Connection { get; set; } = null!;

    private string ConnectionString { get; set; } = null!;
    
    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    public async Task<IQueryRunner> GetRunnerAsync()
    {
        return Runner ??= _factory.CreateQueryRunner(DatabaseType.MySql, ConnectionString);
    }
    
    public async Task InitializeAsync()
    {
        Container = new MySqlBuilder().WithName("MySql-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new MySqlConnection(Container.GetConnectionString());
        await Connection.OpenAsync();
        
        
        await using var command = new MySqlCommand(TestTableSql, Connection);
        await command.ExecuteNonQueryAsync();
    }
    
    private string TestTableSql = """
                                  CREATE TABLE `TestTable` (
                                      `id` BIGINT,
                                      `Name` VARCHAR(20),
                                      `MaxScore` DECIMAL
                                  );

                                  INSERT INTO `TestTable` (`id`, `Name`, `MaxScore`) VALUES 
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
        await Runner.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}