namespace ReData.DemoApp.Endpoints.Datasets.Update;

public sealed record UpdateDataSetResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required Guid DataConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlockResponse> Transformations { get; init; }
}
