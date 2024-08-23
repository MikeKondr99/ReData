using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using ReData.Query.Lang.Expressions;

namespace ReData.Query;

public sealed class Query
{
    public Query(string table, IReadOnlyDictionary<string, ExprType> fields)
    {
        this.Fields = fields;
        this.Table = table;
    }

    // public Query(Query query)
    // {
    //     _commonTables = query._commonTables;
    //     _commonTables ??= new List<Query>();
    //     _commonTables.Add(query);
    //     
    //     query._commonTables = null;
    //     
    //     Select = query.Select;
    //     this.Table = $"CTE{_commonTables.Count}";
    // }
    
    private List<Query>? _commonTables = null;

    public IReadOnlyList<Query>? CommonTables => _commonTables;

    public IReadOnlyDictionary<string, ExprType> Fields { get; init; }
    
    public required IReadOnlyList<OneSelect> Select { get; set; }

    public List<IExpr> Where { get; init; } = new();

    public List<(IExpr, Order)> OrderBy { get; init; } = new();

    public uint Limit { get; set; } = 0;

    public uint Offset { get; set; } = 0;
    
    public string Table { get; private init; }

    public void AddFilters(IEnumerable<string> filter)
    {
        this.Where.AddRange(filter.Select(Expr.Parse));
    }
    
    public void AddOrders(IEnumerable<(string, Order)> orders)
    {
        this.OrderBy.AddRange(orders.Select((t) => (Expr.Parse(t.Item1),t.Item2)));
    }


    public enum Order
    {
        Asc,
        Desc,
    }
}

public record struct OneSelect(string Name, IExpr Value);

public static class ExprExtension
{
    public static OneSelect As(this IExpr expr, string name)
    {
        return new OneSelect(name, expr);

    }
    
}