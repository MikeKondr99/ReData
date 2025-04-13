using System.Collections.ObjectModel;
using System.Text;
using ReData.Core;

namespace ReData.Query.Visitors;

public interface IExpressionCompiler
{
    StringBuilder Compile(StringBuilder builder, IResolvedTemplate node);
}

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
                var unmatched => throw new UnmatchedException<IToken>(unmatched)
            };
        }
        return builder;
    }
}