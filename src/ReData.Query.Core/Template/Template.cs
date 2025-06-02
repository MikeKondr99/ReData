namespace ReData.Query.Core.Template;

public interface ITemplate
{
    public IReadOnlyList<IToken> Tokens { get; }
}

public struct Template : ITemplate
{
    public required IReadOnlyList<IToken> Tokens { get; init; }

    public static implicit operator Template(TemplateInterpolatedStringHandler value)
    {
        return Create(value);
    }

    public static Template Create(TemplateInterpolatedStringHandler value)
    {
        return new Template()
        {
            Tokens = value.Tokens
        };
    }


    public override string ToString()
    {
        return string.Concat(Tokens.Select(t => t switch
        {
            ArgToken(var index) => $"{{{index}}}",
            ConstToken(var text) => text,
        }));
    }
}