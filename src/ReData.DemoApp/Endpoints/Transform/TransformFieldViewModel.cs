using ReData.Query.Core.Types;

namespace ReData.DemoApp.Endpoints.Transform;

public sealed record TransformFieldViewModel
{
    public required string Alias { get; init; }
    public required DataType Type { get; init; }
    public required bool CanBeNull { get; init; }
}