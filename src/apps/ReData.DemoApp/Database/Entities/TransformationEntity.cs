using ReData.DemoApp.Transformations;
using StrictId;

namespace ReData.DemoApp.Database.Entities;

public sealed record TransformationEntity
{
    public required Id<DataSetEntity> DataSetId { get; init; }
    public required uint Order { get; init; }

    public required bool Enabled { get; init; }

    public required string? Description { get; init; }
    public required Transformation Data { get; init; }
}
