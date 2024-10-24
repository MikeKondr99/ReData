using System.Text;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface ILiteralBuilder
{
    StringBuilder String(StringBuilder res, StringLiteral literal);
    StringBuilder Number(StringBuilder res, NumberLiteral literal);
    StringBuilder Integer(StringBuilder res, IntegerLiteral literal);
    StringBuilder Boolean(StringBuilder res, BooleanLiteral literal);
    StringBuilder Null(StringBuilder res, NullLiteral literal);
    StringBuilder Name(StringBuilder res, NameExpr literal);
}

public sealed class GeneratorVisitor : ExprVisitor<StringBuilder>
{
    public required StringBuilder StringBuilder { get; init; }
    public required TypeVisitor TypeVisitor { get; init; }
    public required IFunctionStorage FunctionStorage { get; init; }
    public required ILiteralBuilder LiteralBuilder { get; init; }
    
    public override StringBuilder Visit(IExpr expr)
    {
        return expr switch
        {
            StringLiteral s => LiteralBuilder.String(StringBuilder,s),
            NumberLiteral n => LiteralBuilder.Number(StringBuilder,n),
            IntegerLiteral i => LiteralBuilder.Integer(StringBuilder, i),
            BooleanLiteral b => LiteralBuilder.Boolean(StringBuilder, b),
            NullLiteral n => LiteralBuilder.Null(StringBuilder, n),
            NameExpr n => LiteralBuilder.Name(StringBuilder, n),
            FuncExpr f => Visit(f),
        };
    }

    public override StringBuilder Visit(StringLiteral expr) => LiteralBuilder.String(StringBuilder, expr);

    public override StringBuilder Visit(NumberLiteral expr) => LiteralBuilder.Number(StringBuilder, expr);

    public override StringBuilder Visit(IntegerLiteral expr) => LiteralBuilder.Integer(StringBuilder, expr);

    public override StringBuilder Visit(BooleanLiteral expr) => LiteralBuilder.Boolean(StringBuilder, expr);

    public override StringBuilder Visit(NameExpr expr) => LiteralBuilder.Name(StringBuilder, expr);

    public override StringBuilder Visit(NullLiteral expr) => LiteralBuilder.Null(StringBuilder, expr);

    public override StringBuilder Visit(FuncExpr expr)
    {
        ExprType[] types = new ExprType[expr.Arguments.Length];
        
        for (int i = 0; i < expr.Arguments.Length; i++)
        {
            types[i] = TypeVisitor.Visit(expr.Arguments[i]);
        }

        var sign = new FunctionSignature
        {
            Name = expr.Name,
            ArgumentTypes = types.Select(t => new FunctionArgumentType()
            {
                DataType = t.Type,
                CanBeNull = t.CanBeNull,
            }).ToArray()
        };
        var tokens = FunctionStorage.GetFunction(sign).Template.Tokens;
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