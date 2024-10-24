using System.Globalization;
using System.Text;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class ClickHouseLiteralBuilder : ILiteralBuilder
{
    public StringBuilder String(StringBuilder res, StringLiteral literal) => res.Append($"'{literal.Value}'");

    public StringBuilder Number(StringBuilder res, NumberLiteral literal) => res.Append(literal.Value.ToString("0.0###############",CultureInfo.InvariantCulture));

    
    public StringBuilder Integer(StringBuilder res, IntegerLiteral literal) => res.Append(literal.Value);

    public StringBuilder Boolean(StringBuilder res, BooleanLiteral literal) => res.Append(literal.Value ? "TRUE" : "FALSE");

    public StringBuilder Null(StringBuilder res, NullLiteral literal) => res.Append("NULL");

    public StringBuilder Name(StringBuilder res, NameExpr literal) => res.Append($"\"{literal.Value}\"");
}