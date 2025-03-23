using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface ILiteralBuilder
{
    StringBuilder String(StringBuilder res, StringLiteral literal);
    StringBuilder Number(StringBuilder res, NumberLiteral literal);
    StringBuilder Integer(StringBuilder res, IntegerLiteral literal);
    StringBuilder Boolean(StringBuilder res, BooleanLiteral literal);
    StringBuilder Null(StringBuilder res, NullLiteral literal);
    StringBuilder Name(StringBuilder res, NameExpr literal);
}
