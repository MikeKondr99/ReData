namespace ReData.Query.Core.Value;

public readonly record struct Record(IValue[] values)
{
    public override string ToString() => string.Join(", ", values.AsReadOnly());

    public IValue this[int index] => values[index];

}