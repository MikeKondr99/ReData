using System.Globalization;
using ReData.Query.Runners.Value;

namespace ReData.Query.Core.Value;

public readonly record struct IntegerValue(long Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}