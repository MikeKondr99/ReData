using System.Globalization;
using System.Text;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class MySqlLiteralResolver : ILiteralResolver
{
    
    public Node Resolve(ILiteral literal)
    {
        (TemplateInterpolatedStringHandler template, ExprType type) temp = literal switch
        {
            StringLiteral(var v) => ($"'{v}'", ExprType.Text()),
            NumberLiteral(var v) => (v.ToString("0.0###############", CultureInfo.InvariantCulture), ExprType.Number()),
            IntegerLiteral(var v) => (v.ToString(), ExprType.Integer()),
            BooleanLiteral(var v) => (v ? "0 = 0" : "0 <> 0", ExprType.Boolean()),
            NullLiteral => ("NULL", ExprType.Null())
        };
        return new Node()
        {
            Expression = literal,
            Template = Template.Create(temp.template),
            Type = temp.type,
        };
    }
}