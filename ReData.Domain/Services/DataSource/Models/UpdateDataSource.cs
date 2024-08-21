namespace ReData.Domain.Services.DataSource.Models;

public sealed record UpdateDataSource
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string? Description { get; init; }
    public required Dictionary<string, string?> Parameters { get; init; }
}