using System.Globalization;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class ClickHouseLiteralResolver : ILiteralResolver
{
    public ResolvedExpr Resolve(Literal literal)
    {
        (TemplateInterpolatedStringHandler template, ExprType type) temp = literal switch
        {
            StringLiteral(var v) => ($"'{v}'", ExprType.Text()),
            NumberLiteral(var v) => (v.ToString("0.0###############", CultureInfo.InvariantCulture), ExprType.Number()),
            IntegerLiteral(var v) => (v.ToString(), ExprType.Integer()),
            BooleanLiteral(var v) => (v ? "TRUE" : "FALSE", ExprType.Boolean()),
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