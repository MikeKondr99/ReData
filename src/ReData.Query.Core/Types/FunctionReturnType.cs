namespace ReData.Query.Core.Types;

public sealed record FunctionReturnType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
    
    public bool Aggregated { get; init; }

    public override string ToString()
    {
        if (Aggregated)
        {
            return $"agg<{DataType.Display()}{(CanBeNull ? "" : "!")}>";
        }
        return $"{DataType.Display()}{(CanBeNull ? "" : "!")}";
    }
}