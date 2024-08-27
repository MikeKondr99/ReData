using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;


public sealed class GeneratorVisitor : ExprVisitor<StringBuilder>
{
    
    public override StringBuilder Visit(IExpr expr)
    {
        return expr switch
        {
            StringLiteral s => StringBuilder.Append($"'{s.Value}'"),
            NumberLiteral n => StringBuilder.Append($"{n.Value:0.0}".Replace(",",".")),
            IntegerLiteral i => StringBuilder.Append(i.Value),
            BooleanLiteral b => StringBuilder.Append(b.Value ? "TRUE" : "FALSE"),
            NullLiteral nl => StringBuilder.Append("NULL"),
            NameExpr n => StringBuilder.Append($"\"{n.Value}\""),
            FuncExpr f => Visit(f),
        };
    }
    public required StringBuilder StringBuilder { get; init; }
    public required TypeVisitor TypeVisitor { get; init; }

    public required IFunctionTemplateStorage FunctionTokens { get; init; }
    public override StringBuilder Visit(StringLiteral expr) => StringBuilder.Append($"'{expr.Value}'");

    // TODO можно придумать вариант получше
    public override StringBuilder Visit(NumberLiteral expr) => StringBuilder.Append($"{expr.Value:0.0}".Replace(",","."));

    public override StringBuilder Visit(IntegerLiteral expr) => StringBuilder.Append(expr.Value);

    public override StringBuilder Visit(BooleanLiteral expr) => StringBuilder.Append(expr.Value ? "TRUE" : "FALSE");

    public override StringBuilder Visit(NameExpr expr) => StringBuilder.Append($"\"{expr.Value}\"");

    public override StringBuilder Visit(NullLiteral expr) => StringBuilder.Append("NULL");

    public override StringBuilder Visit(FuncExpr expr)
    {
        ExprType[] types = new ExprType[expr.Arguments.Length];
        
        for (int i = 0; i < expr.Arguments.Length; i++)
        {
            types[i] = TypeVisitor.Visit(expr.Arguments[i]);
        }
        
        var sign = new FunctionSignature(expr.Name, types);
        var tokens = FunctionTokens.GetTemplate(sign).Tokens;
        for (int i = 0; i < tokens.Count; i++)
        {
            _ = tokens[i] switch
            {
                ConstToken(var str) => StringBuilder.Append(str),
                ArgToken(var idx) => Visit(expr.Arguments[idx]),
            };
        }
        return StringBuilder;
    }
}