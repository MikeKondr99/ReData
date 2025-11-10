using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Repositories;

public sealed record DataSet : IEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}