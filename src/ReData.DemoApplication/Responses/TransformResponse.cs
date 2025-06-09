using ReData.Query.Runners.Value;

namespace ReData.DemoApplication.Responses;

public sealed record TransformResponse
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }
    
    public required int Total { get; init; }
    
    public required string Query { get; init; }
    
    public required IReadOnlyList<Dictionary<string, IValue>> Data { get; init; }
    
}