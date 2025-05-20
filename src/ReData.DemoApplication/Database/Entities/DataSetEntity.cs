

using System.Text.Json.Serialization;
// ReSharper disable once CheckNamespace
using System.Collections;

namespace ReData.Database.Entities;

public record DataSetEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required ICollection<TransformationEntity> Transformations { get; init; }
}