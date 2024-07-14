namespace ReData.Database.Entities;

public record CreateTransformationData : ITransformation
{
    public Guid DataSourceId { get; init; }
    
    public required string Table { get; init; }
    
    public required Dictionary<string, FieldType> Fields { get; init; }
}