using MySql.Data.MySqlClient;
using ReData.Query.Runners;
using Testcontainers.MySql;

namespace ReData.Query.Impl.Tests.Fixtures;

public class MySqlDatabaseFixture : IAsyncLifetime, IDatabaseFixture
{
    private Factory _factory = new Factory();
    private MySqlContainer Container { get; set; } = null!;
    private MySqlConnection Connection { get; set; } = null!;

    private string ConnectionString { get; set; } = null!;
    
    private IQueryRunner? Runner = null!; // Runner сохраняется должен быть один потому что он закроет Connection сам
    public Task<IQueryRunner> GetRunnerAsync()
    {
        return Task.FromResult(Runner ??= _factory.CreateQueryRunner(DatabaseType.MySql, ConnectionString));
    }

    public DatabaseType GetDatabaseType()
    {
        return DatabaseType.MySql;
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
                                  CREATE TABLE `User` (
                                      `UserId` INT AUTO_INCREMENT PRIMARY KEY,
                                      `FirstName` TEXT,
                                      `LastName` TEXT,
                                      `Age` INT,
                                      `Salary` DOUBLE,
                                      `DateOfBirth` DATE,
                                      `JoinDate` DATE,
                                      `LastLoginDate` DATE,
                                      `Notes` TEXT
                                  );

                                  INSERT INTO `User` (`FirstName`, `LastName`, `Age`, `Salary`, `DateOfBirth`, `JoinDate`, `LastLoginDate`, `Notes`)
                                  VALUES
                                      ('John', 'Doe', 30, 50000.50, '1990-01-15', '2020-05-10', '2023-10-01', 'Regular user'),
                                      ('Jane', 'Smith', 25, 60000.00, '1995-07-22', '2021-03-15', '2023-09-28', 'Active user'),
                                      ('John', 'Doe', 30, 55000.75, '1990-01-15', '2022-01-20', '2023-10-02', 'Promoted user'),
                                      ('Alice', 'Johnson', 40, 75000.00, '1980-11-30', '2019-11-01', '2023-09-30', 'Manager'),
                                      ('Jane', 'Smith', 25, 62000.50, '1995-07-22', '2021-03-15', '2023-10-03', 'Active user'),
                                      ('Bob', 'Brown', 35, 45000.00, '1985-05-10', '2020-06-01', '2023-09-25', 'New user'),
                                      ('Alice', 'Johnson', 40, 80000.00, '1980-11-30', '2019-11-01', '2023-10-04', 'Senior Manager'),
                                      ('Mike', 'Davis', 28, 48000.00, '1993-02-14', '2021-07-15', '2023-09-29', 'Junior Developer'),
                                      ('Sarah', 'Wilson', 32, 70000.00, '1989-08-20', '2018-12-01', '2023-10-05', 'Team Lead'),
                                      ('John', 'Doe', 30, 60000.00, '1990-01-15', '2020-05-10', '2023-10-06', 'Regular user'),
                                      ('Emily', 'Clark', 27, 52000.00, '1994-03-25', '2022-02-10', '2023-09-27', 'Intern'),
                                      ('Jane', 'Smith', 25, 65000.00, '1995-07-22', '2021-03-15', '2023-10-07', 'Active user'),
                                      ('Chris', 'Evans', 38, 90000.00, '1983-09-12', '2017-10-01', '2023-10-08', 'Director'),
                                      ('Alice', 'Johnson', 40, 85000.00, '1980-11-30', '2019-11-01', '2023-10-09', 'Senior Manager'),
                                      ('Bob', 'Brown', 35, 47000.00, '1985-05-10', '2020-06-01', '2023-10-10', 'New user');
                                  """;


    public async Task DisposeAsync()
    {
        if (Runner is not null)
        {
            await Runner.DisposeAsync();
        }
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}