using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Transformations;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;
using Field = ReData.DemoApplication.Database.Entities.Field;

namespace ReData.DemoApplication.Endpoints.Transform;

public sealed record TransformRequest
{
    public required Guid? TableId { get; init; }
    
    public required IReadOnlyList<Field>? FieldList { get; init; }
    public required List<ITransformation> Transformations { get; init; } = new();
}

public sealed record TransformResponse
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }
    public required int Total { get; init; }
    public required string Query { get; init; }
    public required IReadOnlyList<Dictionary<string, IValue>> Data { get; init; }
}


public sealed record TransformFieldViewModel
{
    public required string Alias { get; init; }
    public required DataType Type { get; init; }
    public required bool CanBeNull { get; init; }
}

// Error response models
public sealed record TransformationErrorResponse
{
    public required int Index { get; init; }
    public required object Errors { get; init; }
}

public sealed record CompilationErrorResponse
{
    public required int Index { get; init; }
    public required string Message { get; init; }
    public required string[]? Query { get; init; }
}

public sealed record ExecutionErrorResponse
{
    public required int Index { get; init; }
    public required string Message { get; init; }
    public required string? Query { get; init; }
}
