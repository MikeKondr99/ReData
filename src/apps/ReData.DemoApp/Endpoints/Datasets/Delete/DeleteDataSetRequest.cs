using Microsoft.AspNetCore.Mvc;

namespace ReData.DemoApp.Endpoints.Datasets.Delete;

public sealed record DeleteDataSetRequest
{
    [FromRoute]
    public required Guid Id { get; init; }
}
