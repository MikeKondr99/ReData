using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components.Implementation;

public sealed class BasicNameResolver(string open, string close) : INameResolver
{
    public TableTemplate ResolveName(ReadOnlySpan<string> path)
    {
        List<IToken> tokens = new List<IToken>();
        foreach (var p in path)
        {
            tokens.Add(new ConstToken(open));
            tokens.Add(new ConstToken(p));
            tokens.Add(new ConstToken(close));
            tokens.Add(new ConstToken("."));
        }
        tokens.RemoveAt(tokens.Count - 1);
        return new TableTemplate(new Template.Template
        {
            Tokens = tokens
        });
    }
}
