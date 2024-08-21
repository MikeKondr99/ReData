using Microsoft.EntityFrameworkCore;

namespace ReData.Domain;

public sealed record DataSet : Entity.IEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required ICollection<Entity.ITransformation> Transformations { get; init; }
}
