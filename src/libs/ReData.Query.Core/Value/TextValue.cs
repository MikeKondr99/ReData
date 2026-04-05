namespace ReData.Query.Core.Value;

public readonly record struct TextValue(string Value) : IValue
{
    public override string ToString() => $"'{Value}'";
}