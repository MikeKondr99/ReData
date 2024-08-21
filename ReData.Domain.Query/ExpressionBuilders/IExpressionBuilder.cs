using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ReData.Domain.Query.Lang.Expressions;

namespace ReData.Domain.Query;

public interface IExpressionBuilder
{
    void Write(StringBuilder res, IExpr expr);
    
    string Build(IExpr expr)
    {
        var res = new StringBuilder();
        Write(res, expr);
        return res.ToString();
    }

}

[Flags]
public enum ExprType
{
    Unknown = 0,
    Null = 1,
    Number = 2,
    Integer = 4,
    String = 8,
    Boolean = 16
}


public record FunctionSignature(string Name, IEnumerable<ExprType> Parameters);


public class FunctionStorage
{
    // private Dictionary<FunctionSignature, ExpressionFunc> storage =
    //     new ()
    //     {
    //         [new ("Upper",[ExprType.String])] = (writer, args) =>
    //             writer
    //             .Add("UPPER(")
    //             .Add(args[0])
    //             .Add(")"),
    //         []
    //         
    //     };
}



public sealed class ExpressionBuilder : IExpressionBuilder
{
    public void Write(StringBuilder res, IExpr expr)
    {
        if (expr is StringLiteral str)
        {
            res.Append($"'{str}'");
        }
        else if (expr is NumberLiteral num)
        {
            res.Append($"{num:0.0}");
        }
        else if (expr is IntegerLiteral inte)
        {
            res.Append($"{inte}");
        }
        else if (expr is BooleanLiteral bl)
        {
            res.Append(bl.Value ? "TRUE" : "FALSE");
        }
        else if (expr is FuncExpr func)
        {
            
        }
    }
    
    
}
