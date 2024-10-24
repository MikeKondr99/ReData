using System.Collections;

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
        }));

    }
}