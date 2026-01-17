using Npgsql;
using Testcontainers.PostgreSql;

namespace ReData.DataExporter.Tests;

#pragma warning disable CA1001
public class CsvFileExportIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private NpgsqlConnection _connection;
    
    public CsvFileExportIntegrationTests()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        // Start the container
        await _postgreSqlContainer.StartAsync();
        
        // Create connection
        _connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _connection.OpenAsync();
        
        // Create test table and insert sample data
        await InitializeTestDataAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
    
    private async Task InitializeTestDataAsync()
    {
        // Create test table
        const string createTableSql = @"
            DROP TABLE IF EXISTS employees;
            CREATE TABLE employees (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                email VARCHAR(100) NOT NULL,
                department VARCHAR(50) NOT NULL,
                salary DECIMAL(10,2) NOT NULL,
                hire_date DATE NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true
            );
        ";
        
        await using NpgsqlCommand createTableCommand = new NpgsqlCommand(createTableSql, _connection);
        await createTableCommand.ExecuteNonQueryAsync();
        
        // Insert sample data with various data types
        const string insertDataSql = @"
            INSERT INTO employees (name, email, department, salary, hire_date, is_active) VALUES
            ('John Smith', 'john.smith@example.com', 'Engineering', 75000.50, '2020-01-15', true),
            ('Sarah Johnson', 'sarah.j@example.com', 'Marketing', 65000.00, '2021-03-22', true),
            ('Michael Brown', 'michael.b@example.com', 'Sales', 55000.75, '2019-11-05', true),
            ('Emily Davis', 'emily.d@example.com', 'HR', 60000.25, '2022-06-10', true),
            ('Robert Wilson', 'robert.w@example.com', 'Engineering', 80000.00, '2018-07-30', false),
            ('Jennifer Lee', 'jennifer.l@example.com', 'Marketing', 70000.50, '2020-09-12', true),
            ('David Miller', 'david.m@example.com', 'Sales', 58000.00, '2023-01-18', true),
            ('Lisa Taylor', 'lisa.t@example.com', 'Engineering', 72000.75, '2021-08-25', true),
            ('Kevin Anderson', 'kevin.a@example.com', 'IT', 90000.00, '2017-12-01', false),
            ('Amanda Clark', 'amanda.c@example.com', 'HR', 62000.25, '2022-02-14', true);
        ";
        
        await using NpgsqlCommand insertDataCommand = new NpgsqlCommand(insertDataSql, _connection);
        await insertDataCommand.ExecuteNonQueryAsync();
    }
    
    [Fact]
    public async Task Export_WithRealPostgresData_GeneratesValidCsv()
    {
        // Arrange
        const string query = @"
            SELECT 
                name,
                email,
                department,
                salary,
                hire_date,
                is_active
            FROM employees 
            ORDER BY id
        ";
        
        await using var command = new NpgsqlCommand(query, _connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        await using (var fileStream = new FileStream("temp.csv", FileMode.Create, FileAccess.Write))
        {
            var csvExporter = new MiniExcelExporter();
            await csvExporter.ExportAsync(reader, fileStream, CancellationToken.None);
        }

        var text = await File.ReadAllTextAsync("temp.csv");
    }
}