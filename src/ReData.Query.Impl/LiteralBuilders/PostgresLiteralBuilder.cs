using System.Globalization;
using System.Text;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class PostgresLiteralBuilder : ILiteralBuilder
{
    public StringBuilder String(StringBuilder res, StringLiteral literal) => res.Append($"'{literal.Value}'");

    public StringBuilder Number(StringBuilder res, RawNumberLiteral literal) => res.Append(literal.Value.ToString("0.0###############",CultureInfo.InvariantCulture));

    
    public StringBuilder Integer(StringBuilder res, RawIntegerLiteral literal) => res.Append(literal.Value);

    public StringBuilder Boolean(StringBuilder res, RawBooleanLiteral literal) => res.Append(literal.Value ? "TRUE" : "FALSE");

    public StringBuilder Null(StringBuilder res, RawNullRawLiteral rawLiteral) => res.Append("NULL");

    public StringBuilder Name(StringBuilder res, NameRawExpr literal) => res.Append($"\"{literal.Value}\"");
}