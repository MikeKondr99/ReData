using System.Globalization;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class SqlServerLiteralResolver : ILiteralResolver
{
    public ResolvedExpr Resolve(Literal literal)
    {
        (TemplateInterpolatedStringHandler template, ExprType type) temp = literal switch
        {
            StringLiteral(var v) => ($"N'{v}'", ExprType.Text()),
            NumberLiteral(var v) => (v.ToString("0.0###############", CultureInfo.InvariantCulture), ExprType.Number()),
            IntegerLiteral(var v) => (v.ToString(), ExprType.Int()),
            BooleanLiteral(var v) => (v ? "0 = 0" : "0 <> 0", ExprType.Boolean()),
            NullLiteral => ("NULL", ExprType.Null())
        };
        return new ResolvedExpr()
        {
            Expression = literal,
            Template = Template.Create(temp.template),
            Type = temp.type.Const(),
        };
    }
}