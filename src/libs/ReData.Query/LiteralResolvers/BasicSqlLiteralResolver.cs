using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.LiteralBuilders;

public abstract class BasicSqlLiteralResolver : ILiteralResolver
{
    public abstract ResolvedExpr Resolve(Literal literal);

    protected ConstToken NameOpen { get; init; } = new ConstToken("\"");
    protected ConstToken NameClose { get; init; } = new ConstToken("\"");
    
    public ResolvedTemplate ResolveName(ReadOnlySpan<string> path)
    {
        List<IToken> tokens = new List<IToken>((path.Length * 4) - 1);
        foreach (var p in path)
        {
            tokens.Add(NameOpen);
            tokens.Add(new ConstToken(p));
            tokens.Add(NameClose);
            tokens.Add(new ConstToken("."));
        }
        tokens.RemoveAt(tokens.Count - 1);
        return new ResolvedTemplate(new Template
        {
            Tokens = tokens
        });
    }
}