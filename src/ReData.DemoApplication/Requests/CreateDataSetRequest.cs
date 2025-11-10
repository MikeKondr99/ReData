using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Requests;

public sealed record CreateDataSetRequest
{
    public required string Name { get; init; }

    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}