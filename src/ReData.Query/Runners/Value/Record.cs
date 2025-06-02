using ReData.Query.Runners.Value;

namespace ReData.Query.Runners.Value;
public readonly record struct Record(IValue[] values)
{
    public override string ToString() => string.Join(", ", values.AsReadOnly());

    public IValue this[int index] => values[index];

}