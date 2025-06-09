using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Responses;

public sealed record TransformFieldViewModel
{
    public required string Alias { get; init; }

    public required DataType Type { get; init; }
    
    public required bool CanBeNull { get; init; }
}