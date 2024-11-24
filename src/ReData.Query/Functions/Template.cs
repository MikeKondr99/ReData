using System.Collections;
using ReData.Core;

namespace ReData.Query.Visitors;

public interface ITemplate
{
    public IReadOnlyList<IToken> Tokens { get; }
}
public struct Template : ITemplate
{
    public required IReadOnlyList<IToken> Tokens { get; init; }
    
    public override string ToString()
    {
        return String.Concat(Tokens.Select(t => t switch
        {
            ArgToken(var index) => $"{{{index}}}",
            ConstToken(var text) => text,
            var unmatched => throw new UnmatchedException<IToken>(unmatched)
        }));

    }
}