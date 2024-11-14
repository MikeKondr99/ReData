
using ReData.Database.Entities;

namespace ReData.Domain;

public sealed record DataSet : IEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required ICollection<ITransformation> Transformations { get; init; }
}
