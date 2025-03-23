using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using Testcontainers.ClickHouse;

namespace ReData.Query.Impl.Tests.Fixtures;

public class ClickHouseDatabaseFixture: IDatabaseFixture 
{
    private QueryServicesFactory _factory = new QueryServicesFactory();
    private ClickHouseContainer Container { get; set; } = null!;
    private ClickHouseConnection Connection { get; set; } = null!;

    private string ConnectionString { get; set; } = null!;
    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    public async Task<IQueryRunner> GetRunnerAsync()
    {
        return Runner ??= _factory.CreateQueryRunner(DatabaseType.ClickHouse, ConnectionString);
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("Создаём ClickHouse");
        Container = new ClickHouseBuilder().WithName("ClickHouse-Tests").Build();
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        Connection = new ClickHouseConnection(ConnectionString);
        await Connection.OpenAsync();
        await Connection.ExecuteStatementAsync(TestTableCreate); 
        await Connection.ExecuteStatementAsync(TestTableFill); 
    }

    private string TestTableCreate = """
                                  CREATE TABLE "TestTable" (
                                      "id" Int32,
                                      "Name" String,
                                      "MaxScore" Decimal(10,2)
                                  ) Engine = MergeTree() ORDER BY id;
                                  """;

    private string TestTableFill =
                                  """
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
                                  """;

    public async Task DisposeAsync()
    {
        await Runner.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}

