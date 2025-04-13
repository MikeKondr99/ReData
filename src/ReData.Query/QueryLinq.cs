using System.Linq.Expressions;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query;

public static class QueryLinq
{
    public static QueryBuilder Where(this QueryBuilder qb, Func<int, string> where)
    {
        return qb.Where(where(0));
    }
    
    public static QueryBuilder Select(this QueryBuilder qb, Func<int, Dictionary<string, string>> select)
    {
        return qb.Select(select(0));
    }
    
    public static QueryBuilder Select(this QueryBuilder qb, Func<int, int> select)
    {
        return qb;
    }

}