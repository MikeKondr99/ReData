using ReData.Database.Entities;

namespace ReData.Application.Controllers.DataSource;

public record DataSourceResponse
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required DataSourceType Type { get;  init; }
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
}