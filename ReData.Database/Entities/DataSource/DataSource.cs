using System.ComponentModel.DataAnnotations.Schema;

namespace ReData.Database.Entities;

public record DataSource : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required DataSourceType Type { get;  init; }
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public required ICollection<DataSourceParameter> Parameters { get; set; }
}