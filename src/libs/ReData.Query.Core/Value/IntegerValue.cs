using System.Globalization;

namespace ReData.Query.Core.Value;

public readonly record struct IntegerValue(long Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}