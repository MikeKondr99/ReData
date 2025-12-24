using ReData.Query.Runners.Value;

namespace ReData.DemoApp.Endpoints.Transform;

public sealed record TransformResponse
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }
    public required long? Total { get; init; }
    public required IReadOnlyList<Dictionary<string, IValue>> Data { get; init; }
}