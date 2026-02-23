namespace ReData.Query.Core.Value;

public readonly record struct NullValue : IValue
{
    public override string ToString() => "NULL";
}