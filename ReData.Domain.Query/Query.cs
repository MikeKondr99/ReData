using System.Runtime.InteropServices;
using ReData.Domain.Query.Lang.Expressions;

namespace ReData.Domain.Query;

public sealed class Query
{
    public Query(string table, IEnumerable<Field> fields)
    {
        this.Fields = fields.ToList();
        this.Table = table;
    }

    public Query(Query query)
    {
        _commonTables = query._commonTables;
        _commonTables ??= new List<Query>();
        _commonTables.Add(query);
        
        query._commonTables = null;
        
        Fields = query.Fields;
        this.Table = $"CTE{_commonTables.Count}";
    }
    
    private List<Query>? _commonTables = null;

    public IReadOnlyList<Query>? CommonTables => _commonTables;

    public IReadOnlyList<Field> Fields { get; set; }

    public List<IExpr> Filters { get; init; } = new();

    public List<(IExpr, Order)> Ordering { get; init; } = new();

    public uint Limit { get; set; } = 0;

    public uint Offset { get; set; } = 0;
    
    public string Table { get; private init; }

    public void AddFilters(IEnumerable<string> filter)
    {
        this.Filters.AddRange(filter.Select(Expr.Parse));
    }
    
    public void AddOrders(IEnumerable<(string, Order)> orders)
    {
        this.Ordering.AddRange(orders.Select((t) => (Expr.Parse(t.Item1),t.Item2)));
    }

    public record struct Field
    {
        public Field(string name)
        {
            Name = name;
        }
        
        public string Name { get; private init; } 
        public IExpr? Value { get; private init; } 
        
        public Field WithValue(string value)
        {
            return this with { Value = Expr.Parse(value) };
        }
    }

    public enum Order
    {
        Asc,
        Desc,
    }
}