using System.Collections;

namespace ReData.Query.Visitors;

public interface ITemplate
{
    public IReadOnlyList<IToken> Tokens { get; }
}
public struct Template : ITemplate
{
    public required IReadOnlyList<IToken> Tokens { get; init; }

    
    public static Template Compile(TemplateInterpolatedStringHandler template)
    {
        return new Template() { Tokens = template.tokens, };
    }
}