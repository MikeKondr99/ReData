using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface IToken;

public record struct ConstToken(string Text) : IToken;

public record struct ArgToken(int Index) : IToken;

public sealed class GeneratorVisitor : ExprVisitor<StringBuilder>
{
    
    public required StringBuilder StringBuilder { get; init; }
    public required TypeVisitor TypeVisitor { get; init; }

    public required IFunctionTemplateStorage FunctionTokens { get; init; }
    public override StringBuilder Visit(StringLiteral expr) => StringBuilder.Append($"'{expr.Value}'");

    // TODO можно придумать вариант получше
    public override StringBuilder Visit(NumberLiteral expr) => StringBuilder.Append($"{expr.Value:0.0}".Replace(",","."));

    public override StringBuilder Visit(IntegerLiteral expr) => StringBuilder.Append(expr.Value);

    public override StringBuilder Visit(BooleanLiteral expr) => StringBuilder.Append(expr.Value ? "TRUE" : "FALSE");

    public override StringBuilder Visit(NameExpr expr) => StringBuilder.Append($"\"expr.Value\"");

    public override StringBuilder Visit(NullLiteral expr) => StringBuilder.Append("NULL");

    public override StringBuilder Visit(FuncExpr expr) 
    {
        var sign = new FunctionSignature(expr.Name, expr.Arguments.Select(a => TypeVisitor.Visit(a)));
        var tokens = FunctionTokens.GetTemplate(sign);
        foreach (var token in tokens)
        {
            _ = token switch
            {
                ConstToken(var str) => StringBuilder.Append(str),
                ArgToken(var idx) => Visit(expr.Arguments[idx]),
            };
        }
        return StringBuilder;
        throw new KeyNotFoundException($"{sign.Name}({String.Join(", ", sign.Parameters)})");
    }
}

public interface ITemplate : IEnumerable<IToken>;

public class Template : ITemplate
{
    private Template(IReadOnlyList<IToken> list)
    {
        this.list = list;
    }
    private IReadOnlyList<IToken> list;
    
    public static Template Compile(TemplateInterpolatedStringHandler template)
    {
        return new Template(template.tokens);
    }
    
    public IEnumerator<IToken> GetEnumerator() => this.list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.list.GetEnumerator();
}

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
        throw new Exception($"Template must only contain Int32 numbers indicating index of function argument, but was {typeof(T)}");
    }
}