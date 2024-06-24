using System.ComponentModel.DataAnnotations.Schema;

namespace ReData.Infrastructure.Entities;

public abstract record DataSource
{
    public Guid Id { get; init; }
    public DataSourceType Type { get; protected init; }
    
    public required string Name { get; init; }
    
    public string? Description { get; init; }
}

public abstract record DataSource<T> : DataSource 
    where T : IDataSourceOptions
{
    public required T Options { get; init; }
}

public interface IDataSourceOptions;

public record PostgresDataSource : DataSource<PostgresDataSourceOptions>
{
    public PostgresDataSource()
    {
        Type = DataSourceType.PostgreSql;
    }
}

public record CsvDataSource : DataSource<CsvDataSourceOptions>
{
    public CsvDataSource()
    {
        Type = DataSourceType.Csv;
    }
    
}

public record PostgresDataSourceOptions : IDataSourceOptions
{
    public required string Host { get; init; }
    
    public uint? Port { get; init; }
    
    public string? Database { get; init; }
    
    public string? Username { get; init; }
    
    public string? Password { get; init; }
    
    // TODO Дополнить список параметров
    // https://www.npgsql.org/doc/connection-string-parameters.html
}

public record CsvDataSourceOptions : IDataSourceOptions
{
    public required string Path { get; init; }
}

public enum DataSourceType
{
    Unknown = 0,// mainly for discrimiantor
    PostgreSql = 1,
    Csv = 2,
    
}