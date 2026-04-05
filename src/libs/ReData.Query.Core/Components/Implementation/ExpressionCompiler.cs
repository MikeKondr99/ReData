using System.Text;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Components.Implementation;

public sealed class ExpressionCompiler : IExpressionCompiler
{

    public StringBuilder Compile(StringBuilder builder, IResolvedTemplate res)
    {
        // TODO раскрыть рекурсию
        var tokens = res.Template.Tokens;
        foreach (var token in tokens)
        {
            _ = token switch
            {
                ConstToken(var str) => builder.Append(str),
                ArgToken(var idx) => res.Arguments is not null ? Compile(builder, res.Arguments[idx]) : throw new Exception($"Template Exception in Compilation {res}"),
            };
        }
        return builder;
    }
}