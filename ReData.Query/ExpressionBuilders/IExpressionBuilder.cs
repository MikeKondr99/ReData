using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query;

public interface IExpressionBuilder
{
    void Write(StringBuilder res, IExpr expr, IReadOnlyDictionary<string, ExprType> fields);
    
    public string Build(IExpr expr, IReadOnlyDictionary<string,ExprType> fields = null!)
    {
        var res = new StringBuilder();
        Write(res, expr, fields ?? new Dictionary<string, ExprType>());
        return res.ToString();
    }

}


public sealed class ExpressionBuilder : IExpressionBuilder
{
    public required FunctionStorage FunctionStorage { get; init; }
    
    public void Write(StringBuilder res, IExpr expr, IReadOnlyDictionary<string, ExprType> fields)
    {
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = fields,
            FunctionTypes = FunctionStorage,
        };
        
        var visitor = new GeneratorVisitor()
        {
            StringBuilder = res,
            FunctionTokens = FunctionStorage,
            TypeVisitor = typeVisitor,
        };

        visitor.Visit(expr);
    }
        
        

    
    
}
