using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface ILiteralBuilder
{
    StringBuilder String(StringBuilder res, StringLiteral literal);
    StringBuilder Number(StringBuilder res, RawNumberLiteral literal);
    StringBuilder Integer(StringBuilder res, RawIntegerLiteral literal);
    StringBuilder Boolean(StringBuilder res, RawBooleanLiteral literal);
    StringBuilder Null(StringBuilder res, RawNullRawLiteral rawLiteral);
    StringBuilder Name(StringBuilder res, NameRawExpr literal);
}
