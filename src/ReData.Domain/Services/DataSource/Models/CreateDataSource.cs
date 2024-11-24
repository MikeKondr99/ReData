using ReData.Database.Entities;

namespace ReData.Domain.Services.DataSource.Models;

public sealed record CreateDataSource
{
    public required string Name { get; init; }
    
    public required string? Description { get; init; }
    
    public required DataSourceType Type { get; init; }
    
    public required Dictionary<string, string> Parameters { get; init; }
}