namespace ReData.Query.Impl.Runners;

public readonly record struct NullValue : IValue
{
    public override string ToString() => "NULL";
}