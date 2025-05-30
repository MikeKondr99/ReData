using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReData.Query.Lang.Expressions;

public sealed record NumberLiteral : Literal<double>
{
    [SetsRequiredMembers]
    public NumberLiteral(double value) : base(value) {}

    public override string ToString()
    {
        return Value.ToString();
    }

    public new void Deconstruct(out double value)
    {
        value = Value;
    }
}