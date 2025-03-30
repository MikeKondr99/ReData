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
    void Write(StringBuilder res, IExpr expr, IFieldStorage fields);
    
    public string Build(IExpr expr, IFieldStorage fields)
    {
        var res = new StringBuilder();
        Write(res, expr, fields);
        return res.ToString();
    }

}


public sealed class ExpressionCompiler : IExpressionBuilder
{
    public required IFunctionStorage FunctionStorage { get; init; }
    
    public required ILiteralBuilder LiteralBuilder { get; init; }
    
    
    public void Write(StringBuilder res, IExpr expr, IFieldStorage fields)
    {
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = fields,
            FunctionTypes = FunctionStorage,
        };
        
        var visitor = new GeneratorVisitor()
        {
            StringBuilder = res,
            FunctionStorage = FunctionStorage,
            TypeVisitor = typeVisitor,
            LiteralBuilder = LiteralBuilder
        };

        visitor.Visit(expr);
    }
        
        

    
    
}
