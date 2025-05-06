namespace ReData.Query.Runners.Value;

public readonly record struct TextValue(string Value) : IValue
{
    public override string ToString() => $"'{Value}'";
}