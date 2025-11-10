using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Endpoints.Functions;

public sealed record FunctionResponse
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }
    
    public required string DisplayText { get; init; }
}