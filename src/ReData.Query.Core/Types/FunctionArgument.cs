namespace ReData.Query.Core.Types;

public record FunctionArgument
{
    public required string Name { get; init; }

    public required FunctionArgumentType Type { get; init; }
    
    public required bool PropagateNull { get; init; }

    public override string ToString()
    {
        return $"{Name}: {Type}";
    }
}

public record FunctionArgumentType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }

    public override string ToString()
    {
        return $"{DataType.Display()}{(CanBeNull ? "" : "!")}";
    }
}