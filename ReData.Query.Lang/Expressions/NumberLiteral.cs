using System.Globalization;

namespace ReData.Query.Lang.Expressions;

public record struct NumberLiteral(double Value) : ILiteral<double>
{
    public override string ToString()
    {
        return Value.ToString();
    }
}