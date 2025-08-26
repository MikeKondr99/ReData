using ReData.DemoApplication.Repositories;

namespace ReData.DemoApplication.Database.Entities;

public record DataSetEntity : IEntity
{
    public required Guid Id { get; set; }
    
    public required string Name { get; set; }

    public required List<TransformationEntity> Transformations { get; set; }
}