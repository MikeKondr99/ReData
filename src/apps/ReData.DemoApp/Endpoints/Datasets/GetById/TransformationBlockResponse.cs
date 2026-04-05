using System.ComponentModel.DataAnnotations;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Datasets;

public sealed record TransformationBlockResponse
{
    public required bool Enabled { get; init; }
    public required string? Description { get; init; }
    public required Transformation Transformation { get; init; }
}
