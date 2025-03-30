using System.Globalization;

namespace ReData.Query.Lang.Expressions;

public record struct RawNumberLiteral(double Value) : ILiteral<double>
{
    public override string ToString()
    {
        return Value.ToString();
    }
}