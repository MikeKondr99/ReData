using System.Runtime.CompilerServices;

namespace ReData.Query.Core.Template;

[InterpolatedStringHandler]
public struct TemplateInterpolatedStringHandler
{
    public List<IToken> Tokens { get; set; }

    public TemplateInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        Tokens = new List<IToken>(literalLength + formattedCount);
    }

    public void AppendLiteral(string s)
    {
        Tokens.Add(new ConstToken(s));
    }
    
    public void AppendFormatted(ITemplate? template)
    {
        if (template is not null)
        {
            Tokens.AddRange(template.Tokens);
        }
    }

    public void AppendFormatted<T>(T t)
    {
        if (t is int idx)
        {
            Tokens.Add(new ArgToken(idx));
            return;
        }

        if (t is string cnst)
        {
            Tokens.Add(new ConstToken(cnst));
            return;
        }
        throw new Exception($"Template must only contain 'int' or 'string', but was {typeof(T)}");
    }
    
    public Template Compile()
    {
        return new Template() { Tokens = this.Tokens, };
    }

    public static implicit operator TemplateInterpolatedStringHandler(string text)
    {
        return new TemplateInterpolatedStringHandler()
        {
            Tokens = [new ConstToken(text)]
        };
    }
}

