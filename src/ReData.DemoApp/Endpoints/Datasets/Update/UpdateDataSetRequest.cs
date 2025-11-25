using Microsoft.AspNetCore.Mvc;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

public sealed record UpdateDataSetRequest
{
    [FromRoute]
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    
    public required Guid ConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}