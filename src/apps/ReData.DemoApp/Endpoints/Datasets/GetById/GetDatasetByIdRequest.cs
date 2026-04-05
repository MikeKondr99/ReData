using Microsoft.AspNetCore.Mvc;

namespace ReData.DemoApp.Endpoints.Datasets.GetById;

public record GetDatasetByIdRequest
{
    [FromRoute]
    public required Guid Id { get; init; }
}
