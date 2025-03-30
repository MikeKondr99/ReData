using System.Text;
using ReData.Core;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public sealed class GeneratorVisitor : ExprVisitor<StringBuilder>
{
    public required StringBuilder StringBuilder { get; init; }
    public required TypeVisitor TypeVisitor { get; init; }
    public required IFunctionStorage FunctionStorage { get; init; }
    public required ILiteralBuilder LiteralBuilder { get; init; }

    public override StringBuilder Visit(IRawExpr rawExpr)
    {
        return rawExpr switch
        {
            StringLiteral s => LiteralBuilder.String(StringBuilder, s),
            RawNumberLiteral n => LiteralBuilder.Number(StringBuilder, n),
            RawIntegerLiteral i => LiteralBuilder.Integer(StringBuilder, i),
            RawBooleanLiteral b => LiteralBuilder.Boolean(StringBuilder, b),
            RawNullRawLiteral n => LiteralBuilder.Null(StringBuilder, n),
            NameRawExpr n => LiteralBuilder.Name(StringBuilder, n),
            FuncRawExpr f => Visit(f),
            var unmatched => throw new UnmatchedException<IRawExpr>(unmatched)
        };
    }

    public override StringBuilder Visit(StringLiteral expr) => LiteralBuilder.String(StringBuilder, expr);

    public override StringBuilder Visit(RawNumberLiteral expr) => LiteralBuilder.Number(StringBuilder, expr);

    public override StringBuilder Visit(RawIntegerLiteral expr) => LiteralBuilder.Integer(StringBuilder, expr);

    public override StringBuilder Visit(RawBooleanLiteral expr) => LiteralBuilder.Boolean(StringBuilder, expr);

    public override StringBuilder Visit(NameRawExpr rawExpr) => LiteralBuilder.Name(StringBuilder, rawExpr);

    public override StringBuilder Visit(RawNullRawLiteral expr) => LiteralBuilder.Null(StringBuilder, expr);

    public override StringBuilder Visit(FuncRawExpr rawExpr)
    {
        ExprType[] types = new ExprType[rawExpr.Arguments.Count];

        for (int i = 0; i < rawExpr.Arguments.Count; i++)
        {
            types[i] = TypeVisitor.Visit(rawExpr.Arguments[i]);
        }

        var sign = new FunctionSignature
        {
            Name = rawExpr.Name,
            Kind = rawExpr.Kind switch
            {
                FuncExprKind.Binary => FunctionKind.Binary,
                FuncExprKind.Method => FunctionKind.Method,
                FuncExprKind.Unary => FunctionKind.Unary,
                FuncExprKind.Default => FunctionKind.Default,
                var a => (FunctionKind)a,
            },
            ArgumentTypes = types.Select(t => new FunctionArgumentType()
            {
                DataType = t.Type,
                CanBeNull = t.CanBeNull,
            }).ToArray()
        };
        var res = FunctionStorage.ResolveFunction(sign);
        if (res is null)
        {
            throw new Exception($"function `{sign}` not found");
        }

        var tokens = res.GetTokens();

        foreach (var token in tokens)
        {
            _ = token switch
            {
                ConstToken(var str) => StringBuilder.Append(str),
                ArgToken(var idx) => Visit(rawExpr.Arguments[idx]),
                var unmatched => throw new UnmatchedException<IToken>(unmatched)
            };

        }
        return StringBuilder;
    }
}