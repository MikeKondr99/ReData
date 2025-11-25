using Microsoft.AspNetCore.Mvc;

namespace ReData.DemoApp.Endpoints.Datasets.GetById;

public record GetByIdRequest
{
    [FromRoute]
    public required Guid Id { get; init; }
}