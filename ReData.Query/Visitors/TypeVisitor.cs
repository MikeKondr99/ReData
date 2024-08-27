using System.Runtime.CompilerServices;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public class TypeVisitor : ExprVisitor<ExprType>
{
    public required IReadOnlyDictionary<string, ExprType> FieldTypes { get; init; }
    
    public required IFunctionTypesStorage FunctionTypes { get; init; }

    private Dictionary<IExpr, ExprType> _cache = new Dictionary<IExpr, ExprType>();

    public override ExprType Visit(IExpr expr)
    {
        if (_cache.TryGetValue(expr, out var value))
            return value;
        var result = expr switch
        {
            StringLiteral => ExprType.String,
            NumberLiteral => ExprType.Number,
            IntegerLiteral => ExprType.Integer,
            BooleanLiteral => ExprType.Boolean,
            NullLiteral => ExprType.Null,
            NameExpr n => FieldTypes[n.Value],
            FuncExpr f => Visit(f),
        };
        _cache[expr] = result;
        return result;
    }
    
    public override ExprType Visit(StringLiteral expr) => ExprType.String;
    public override ExprType Visit(NumberLiteral expr) => ExprType.Number;
    public override ExprType Visit(IntegerLiteral expr) => ExprType.Integer;
    public override ExprType Visit(BooleanLiteral expr) => ExprType.Boolean;
    
    public override ExprType Visit(NullLiteral expr) => ExprType.Null;
    
    public override ExprType Visit(FuncExpr expr)
    {
        ExprType[] types = new ExprType[expr.Arguments.Length];
        
        for (int i = 0; i < expr.Arguments.Length; i++)
        {
            types[i] = Visit(expr.Arguments[i]);
        }
        
        var sign = new FunctionSignature(expr.Name, types);
        // var sign = new FunctionSignature(expr.Name, expr.Arguments.Select(Visit));
        var type = FunctionTypes.GetType(sign);
        return type;
    }

    public override ExprType Visit(NameExpr name)
    {
        return FieldTypes[name.Value];
    }
    
    
}