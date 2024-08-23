using System.Runtime.CompilerServices;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public class TypeVisitor : ExprVisitor<ExprType>
{
    public required IReadOnlyDictionary<string, ExprType> FieldTypes { get; init; }
    
    public required IFunctionTypesStorage FunctionTypes { get; init; }
    
    public override ExprType Visit(StringLiteral expr) => ExprType.String;
    public override ExprType Visit(NumberLiteral expr) => ExprType.Number;
    public override ExprType Visit(IntegerLiteral expr) => ExprType.Integer;
    public override ExprType Visit(BooleanLiteral expr) => ExprType.Boolean;
    
    public override ExprType Visit(NullLiteral expr) => ExprType.Null;
    
    public override ExprType Visit(FuncExpr expr)
    {
        var sign = new FunctionSignature(expr.Name, expr.Arguments.Select(Visit));
        return FunctionTypes.GetType(sign);
    }

    public override ExprType Visit(NameExpr name)
    {
        return FieldTypes[name.Value];
    }
    
    
}