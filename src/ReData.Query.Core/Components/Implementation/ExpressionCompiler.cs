using System.Text;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Components.Implementation;

public sealed class ExpressionCompiler : IExpressionCompiler
{

    public StringBuilder Compile(StringBuilder builder, IResolvedTemplate node)
    {
        // TODO раскрыть рекурсию
        var tokens = node.Template.Tokens;
        foreach (var token in tokens)
        {
            _ = token switch
            {
                ConstToken(var str) => builder.Append(str),
                ArgToken(var idx) => node.Arguments is not null ? Compile(builder, node.Arguments[idx]) : throw new Exception($"Template Exception in Compilation {node}"),
            };
        }
        return builder;
    }
}