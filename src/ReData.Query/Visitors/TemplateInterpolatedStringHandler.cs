using System.Runtime.CompilerServices;

namespace ReData.Query.Visitors;

[InterpolatedStringHandler]
public struct TemplateInterpolatedStringHandler
{
    public List<IToken> tokens;

    public TemplateInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        tokens = new List<IToken>(literalLength + formattedCount);
    }

    public void AppendLiteral(string s)
    {
        tokens.Add(new ConstToken(s));
    }

    public void AppendFormatted<T>(T t)
    {
        if (t is int idx)
        {
            tokens.Add(new ArgToken(idx));
            return;
        }

        if (t is string cnst)
        {
            tokens.Add(new ConstToken(cnst));
            return;
        }
        throw new Exception($"Template must only contain Int32 numbers indicating index of function argument, but was {typeof(T)}");
    }
    
    public Template Compile()
    {
        return new Template() { Tokens = this.tokens, };
    }

    public static implicit operator TemplateInterpolatedStringHandler(string text)
    {
        return new TemplateInterpolatedStringHandler()
        {
            tokens = [new ConstToken(text)]
        };
    }
}

