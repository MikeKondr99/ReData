namespace ReData.Query.Functions;

public record FunctionArgument
{
    public required string Name { get; init; }

    public required FunctionArgumentType Type { get; init; }
}

public record FunctionArgumentType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }

    public override string ToString()
    {
        return $"{DataType}{(CanBeNull ? "?" : "")}";
    }
}