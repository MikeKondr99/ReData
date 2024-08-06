using ReData.Core;
using ReData.Database.Entities;

namespace ReData.Application.Controllers.DataSource;

public record CreateDataSourceRequest
{
    public required DataSourceType Type { get;  init; }
    
    public required string Name { get; init; }
    
    public string? Description { get; init; }
    
    public required Dictionary<string, string> Parameters { get; init; }
}

public record UpdateDataSourceRequest
{
    // public required Guid Id { get; init; }

    public required string Name { get; init; }
    
    public string? Description { get; init; }
    
    public required Dictionary<string, string> Parameters { get; init; }
}

public record DeleteDataSourceRequest
{
    public required Guid Id { get; init; }
}
