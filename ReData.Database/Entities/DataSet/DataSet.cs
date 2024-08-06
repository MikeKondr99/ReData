// ReSharper disable once CheckNamespace

namespace ReData.Database.Entities;

public record DataSet : IEntity
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }

    public Guid DataSourceId { get; init; } // Уберем когда будут готовы трансформации
    
    public required string Table { get; init; } // Уберем когда будут готовы трансформации
    public required ICollection<FieldType> Fields { get; init; } // После трансформаций будет служить кэшем
}

public record Field
{
    public required string Name { get; init; }

    public required FieldType Type { get; init; }
}

public enum FieldType
{
    Unknown = 0,
    Boolean = 1,
    Integer = 2,
    Real = 3,
    Text = 4,
    Date = 5
}