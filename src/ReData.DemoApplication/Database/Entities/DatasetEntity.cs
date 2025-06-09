using ReData.DemoApplication.Repositories;

namespace ReData.DemoApplication.Database.Entities;

public record DataSetEntity : IEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required ICollection<TransformationEntity> Transformations { get; init; }
}