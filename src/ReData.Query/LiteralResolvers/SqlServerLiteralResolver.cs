using System.Buffers;
using System.Globalization;
using System.Text;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.LiteralBuilders;

public sealed class SqlServerLiteralResolver : BasicSqlLiteralResolver
{
    public override ResolvedExpr Resolve(Literal literal)
    {
        (TemplateInterpolatedStringHandler template, ExprType type) temp = literal switch
        {
            StringLiteral(var v) => (EscapeString(v), ExprType.Text()),
            NumberLiteral(var v) => (v.ToString("0.0###############", CultureInfo.InvariantCulture), ExprType.Number()),
            IntegerLiteral(var v) => (v.ToString(CultureInfo.InvariantCulture), ExprType.Int()),
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
    
    private static SearchValues<char> escapeValues = SearchValues.Create("'\n\r");

    private static string EscapeString(string text)
    {
        if (!text.AsSpan().ContainsAny(escapeValues))
        {
            return $"N'{text}'";
        }

        StringBuilder sb = new StringBuilder(text.Length + 5);
        sb.Append("N'");
        foreach (var symbol in text)
        {
            sb = symbol switch
            {
                '\'' => sb.Append(@"''"),
                '\n' => sb.Append(@"' + CHAR(10) + N'"),
                '\r' => sb.Append(@"' + CHAR(13) + N'"),
                _ => sb.Append(symbol),
            };
        }
        sb.Append('\'');
        return sb.ToString();
    }
}