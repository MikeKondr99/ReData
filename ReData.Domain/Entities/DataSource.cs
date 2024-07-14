
using ReData.Database.Entities;

namespace ReData.Domain;

public sealed record DataSource : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required DataSourceType Type { get;  init; }
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public required Dictionary<string, string> Parameters { get; set; }
}
