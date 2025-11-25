namespace ReData.DemoApp.Endpoints.Datasets.GetById;

public sealed record DataSetResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    
    public required Guid DataConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlockResponse> Transformations { get; init; }
}