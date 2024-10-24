using System.Runtime.CompilerServices;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public class TypeVisitor : ExprVisitor<ExprType>, ITypeVisitor
{
    public required IFieldStorage FieldTypes { get; init; }
    
    public required IFunctionStorage FunctionTypes { get; init; }

    private Dictionary<IExpr, ExprType> _cache = new Dictionary<IExpr, ExprType>();

    public override ExprType Visit(IExpr expr)
    {
        if (_cache.TryGetValue(expr, out var value))
            return value;
        var result = expr switch
        {
            StringLiteral => ExprType.Text().Const(),
            NumberLiteral => ExprType.Number().Const(),
            IntegerLiteral => ExprType.Integer().Const(),
            BooleanLiteral => ExprType.Boolean().Const(),
            NullLiteral => ExprType.Null().Const(),
            NameExpr n => FieldTypes.GetType(n.Value),
            FuncExpr f => Visit(f),
        };
        _cache[expr] = result;
        return result;
    }

    public override ExprType Visit(StringLiteral expr) => ExprType.Text();
    public override ExprType Visit(NumberLiteral expr) => ExprType.Number();
    public override ExprType Visit(IntegerLiteral expr) => ExprType.Integer();
    public override ExprType Visit(BooleanLiteral expr) => ExprType.Boolean();
    
    public override ExprType Visit(NullLiteral expr) => ExprType.Null();
    
    public override ExprType Visit(FuncExpr expr)
    {
        ExprType[] types = new ExprType[expr.Arguments.Length];
        
        for (int i = 0; i < expr.Arguments.Length; i++)
        {
            types[i] = Visit(expr.Arguments[i]);
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
        var type = FunctionTypes.GetFunction(sign).ReturnType;
        return new ExprType()
        {
            Type = type.DataType,
            CanBeNull = type.CanBeNull,
            IsConstant = types.All(t => t.IsConstant),
        };
    }

    public override ExprType Visit(NameExpr name)
    {
        return FieldTypes.GetType(name.Value);
    }
    
    
}

public interface ITypeVisitor : IExprVisitor<ExprType>
{
    
}