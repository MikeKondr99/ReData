namespace ReData.Domain;

public sealed record DataSet : Entity.IEntity
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }

    public Guid DataSourceId { get; init; } // Уберем когда будут готовы трансформации
    
    public required string Table { get; init; } // Уберем когда будут готовы трансформации
    
    public required ICollection<Entity.FieldType> Fields { get; init; } // После трансформаций будет служить кэшем
}