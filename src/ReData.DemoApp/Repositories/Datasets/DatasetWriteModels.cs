using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Repositories.Datasets;

public sealed record CreateDatasetData
{
    public required string Name { get; init; }

    public required Guid ConnectorId { get; init; }

    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}

public sealed record UpdateDatasetData
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid ConnectorId { get; init; }

    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}

