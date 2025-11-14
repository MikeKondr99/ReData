
using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Database.Entities;

public sealed record TransformationEntity
{
    public required Guid DataSetId { get; init; }
    public required uint Order { get; init; }
    
    public required bool Enabled { get; init; }
    
    public required string? Description { get; init; }
    public required ITransformation Data { get; init; }
}