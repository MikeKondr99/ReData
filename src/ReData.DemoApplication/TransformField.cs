using ReData.Query.Core.Types;

namespace ReData.DemoApplication;

public sealed record TransformField
{
    public required string Alias { get; init; }

    public required DataType Type { get; init; }
    
    public required bool CanBeNull { get; init; }
}