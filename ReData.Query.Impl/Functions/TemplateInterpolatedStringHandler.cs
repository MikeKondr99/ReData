using System.Runtime.CompilerServices;

namespace ReData.Query.Visitors;

[InterpolatedStringHandler]
public ref struct TemplateInterpolatedStringHandler
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

        if (t is IArg arg)
        {
            tokens.Add(new ArgToken(arg.Index));
            return;
        }
        throw new Exception($"Template must only contain Int32 numbers indicating index of function argument, but was {typeof(T)}");
    }
    
    public Template Compile()
    {
        return new Template() { Tokens = this.tokens, };
    }
}