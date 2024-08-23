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
    void Write(StringBuilder res, IExpr expr, Dictionary<string, ExprType> fields);
    
    string Build(IExpr expr, Dictionary<string,ExprType> fields = null!)
    {
        var res = new StringBuilder();
        Write(res, expr, fields ?? new ());
        return res.ToString();
    }

}


public sealed class ExpressionBuilder : IExpressionBuilder
{
    public required FunctionStorage FunctionStorage { get; init; }
    
    public void Write(StringBuilder res, IExpr expr, Dictionary<string, ExprType> fields)
    {
        FunctionStorage functions =
        [
            new () {
                Name = "Num",
                Parameters = [ExprType.String],
                ReturnType = ExprType.OptionalNumber,
                Template = Template.Compile($"CAST({0} AS DECIMAL)")
            },
            new () {
                Name = "Coalesce",
                Parameters = [ExprType.OptionalNumber,ExprType.Number],
                ReturnType = ExprType.Number,
                Template = Template.Compile($"COALESCE({0}, {1})"),
            },
            new () {
                Name = "+",
                Parameters = [ExprType.Number,ExprType.Number],
                ReturnType = ExprType.Number,
                Template = Template.Compile($"({0} + {1})")
            }
        ];
        
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = fields,
            FunctionTypes = functions,
        };
        
        var visitor = new GeneratorVisitor()
        {
            StringBuilder = res,
            FunctionTokens = functions,
            TypeVisitor = typeVisitor,
        };

        visitor.Visit(expr);
    }
        
        

    
    
}
