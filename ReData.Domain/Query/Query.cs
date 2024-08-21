using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Primitives;

namespace ReData.Domain.Query;

public interface IQueryBuilder
{
    Entity.DataSourceType Type { get; }
    string Build(Query query);

}

public abstract class SqlQueryBuilder : IQueryBuilder
{
    public abstract Entity.DataSourceType Type { get; }
    
    public virtual string Build(Query query)
    {
        StringBuilder res = new StringBuilder();
        WriteQuery(res,query);
        return res.ToString();
    }

    private void WriteQuery(StringBuilder res, Query query)
    {
        WriteCommonTables(res, query);
        WriteSelect(res, query);
        WriteFilters(res, query);
        WriteLimit(res, query);
    }

    private void WriteLimit(StringBuilder res, Query query)
    {
        if (query.Limit > 0)
        {
            res.Append($"LIMIT {query.Limit}\n");
        }
        
        if (query.Offset > 0)
        {
            res.Append($"OFFSET {query.Offset}\n");
        }
    }

    private void WriteCommonTables(StringBuilder res, Query query)
    {
        if (query.CommonTables is null) return;
        
        int last = query.CommonTables.Count() - 1;
        res.Append("WITH ");
        var _ = query.CommonTables.Select((Query cte,int i) =>
        {
            res.Append($"\"CTE{i + 1}\" AS (\n");
            WriteQuery(res,cte);
            return res.Append(i != last ? "),\n" : ")\n");
        });
    }
    
    private void WriteSelect(StringBuilder res, Query query)
    {
        res.Append("SELECT ");
        if (query.Fields.Count == 0)
        {
            res.Append('*');
        }
        else
        {
            WriteFields(res, query);
        }
        res.Append($" FROM {query.Table}\n");
        WriteExpression(res, $"[{query.Table}]");
        res.Append('\n');
    }

    private void WriteFields(StringBuilder res, Query query)
    {
        int last = query.Fields.Count - 1;

        for (var i = 0; i < query.Fields.Count; i++)
        {
            var field = query.Fields[i];
            
            WriteExpression(res,$"[{field.Value}]");
            res.Append(" AS ");
            WriteExpression(res, field.Name);
            if (i != last)
            {
                res.Append(", ");
            }
        }
    }

    private void WriteExpression(StringBuilder res, string expression)
    {
        res.Append($"({expression})");
    }

    private void WriteFilters(StringBuilder res, Query query)
    {
        foreach (var filter in query.Filters)
        {
            res.Append("WHERE ");
            WriteExpression(res, filter);
            res.Append('\n');
        }
    }
}



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

    public List<string> Filters { get; init; } = new();

    public uint Limit { get; set; } = 0;

    public uint Offset { get; set; } = 0;
    
    public string Table { get; private init; }

    public void AddFilters(IEnumerable<string> filter)
    {
        this.Filters.AddRange(filter);
    }

    public record struct Field
    {
        public Field(string name)
        {
            Name = name;
            Value = name;
        }
        
        public string Name { get; private init; } 
        public string Value { get; private init; } 
        
        public Field WithValue(string value)
        {
            return this with { Value = value };
        }
    }
}