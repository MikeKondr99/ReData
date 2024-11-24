using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query;

public sealed record Query : IQuerySource
{
    public IQuerySource From { get; init; } = new NoSource();

    public IReadOnlyList<Map>? Select { get; init; }

    public IReadOnlyList<IExpr>? Where { get; init; }

    public IReadOnlyList<Order>? OrderBy { get; init; }

    public uint Limit { get; init; }

    public uint Offset { get; init; }

    public record struct Order(IExpr Expr, Order.Type Direction)
    {
        public enum Type
        {
            Asc,
            Desc
        }
    }
    
    public record struct Field(string Name, ExprType Type);
    public record struct Map(string Name, IExpr Expr);

    public string Name => $"STEP{No}";

    public int No { get; init; } = 1;

    public IFieldStorage Fields(IFunctionStorage functions)
    {
        var fields = this.From.Fields(functions);
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = fields,
            FunctionTypes = functions,
        };
        
        if (Select is null) return fields;

        var result = Select.Select(m => new Field(m.Name,typeVisitor.Visit(m.Expr)));
        return new FieldStorage(result.ToArray());
    }
    
    private record struct NoSource() : IQuerySource
    {
        public string? Name => null;
        public IFieldStorage Fields(IFunctionStorage functions) => new FieldStorage([]);
    }
}