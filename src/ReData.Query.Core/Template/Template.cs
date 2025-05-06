namespace ReData.Query.Core.Template;

public interface ITemplate
{
    public IReadOnlyList<IToken> Tokens { get; }
}
public struct Template : ITemplate
{
    public required IReadOnlyList<IToken> Tokens { get; init; }
    
    

    public static Template Create(TemplateInterpolatedStringHandler value)
    {
        return new Template()
        {
            Tokens = value.tokens
        };
    }

    public static implicit operator Template(TemplateInterpolatedStringHandler value)
    {
        return Template.Create(value);
    }
    
    
    public override string ToString()
    {
        return String.Concat(Tokens.Select(t => t switch
        {
            ArgToken(var index) => $"{{{index}}}",
            ConstToken(var text) => text,
        }));

    }

}