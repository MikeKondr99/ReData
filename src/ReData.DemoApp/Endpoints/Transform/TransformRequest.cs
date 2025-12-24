using FluentValidation;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Transform;

public sealed record TransformRequest
{
    public required Guid DataConnectorId { get; init; }

    public required uint PageNumber { get; init; }

    public required uint PageSize { get; init; }

    public string? OrderByName { get; init; }

    public bool? OrderByDescending { get; init; }

    public required List<ITransformation> Transformations { get; init; } = new();
}