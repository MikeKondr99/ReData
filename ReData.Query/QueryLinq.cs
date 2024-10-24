using ReData.Query.Lang.Expressions;

namespace ReData.Query;

public static class QueryLinq
{
    public static Query Where(this Query query, string predicate)
    {
        if (query.Select is not null || query.Limit > 0 || query.Offset > 0)
        {
            return new Query()
            {
                No = query.No + 1,
                From = query,
                Where = [Expr.Parse(predicate)]
            };
        }
        return query with
        {
            Where = [..query.Where ?? [], Expr.Parse(predicate)]
        };
    }

    private static Query Order(this Query query, string expr, Query.Order.Type type)
    {
        if (query.Select is not null || query.Limit > 0 || query.Offset > 0)
        {
            return new Query()
            {
                No = query.No + 1,
                From = query,
                OrderBy = [new Query.Order(Expr.Parse(expr),type)]
            };
        }
        return query with
        {
            OrderBy = [new Query.Order(Expr.Parse(expr),type)]
        };
        
    }

    public static Query OrderBy(this Query query, string expr)
    {
        return Order(query, expr, Query.Order.Type.Asc);
    }
    
    public static Query OrderByDescending(this Query query, string expr)
    {
        return Order(query, expr, Query.Order.Type.Desc);
    }
    public static Query ThenBy(this Query query, string expr)
    {
        if (query.OrderBy?.Count is not null or 0)
        {
            return query with
            {
                OrderBy = [..query.OrderBy,new Query.Order(Expr.Parse(expr),Query.Order.Type.Asc)]
            };
        }
        throw new Exception("Can't use ThenBy on query without ordering");
    }
    
    public static Query ThenByDescending(this Query query, string expr)
    {
        if (query.OrderBy?.Count is not null or 0)
        {
            return query with
            {
                OrderBy = [..query.OrderBy,new Query.Order(Expr.Parse(expr),Query.Order.Type.Desc)]
            };
        }
        throw new Exception("Can't use ThenByDescending on query without ordering");
    }

    public static Query Select(this Query query, Dictionary<string, string> select)
    {
        if (query.Select is not null || query.Where?.Count > 0 || query.OrderBy?.Count > 0 )
        {
            return new Query()
            {
                No = query.No + 1,
                From = query,
                Select = select.Select(kv => new Query.Map(kv.Key,Expr.Parse(kv.Value))).ToArray(),
            };
        }
        return query with
        {
            Select = select.Select(kv => new Query.Map(kv.Key,Expr.Parse(kv.Value))).ToArray(),
        };
    }
    
    public static Query Take(this Query query, uint take)
    {
        if (query.Limit > 0)
        {
            take = Math.Min(query.Limit, take);
        }
        return query with
        {
            Limit = take,
        };
    }
    
    public static Query Skip(this Query query, uint skip)
    {
        var offset = skip + query.Offset;
        if (query.Limit == 0)
        {
            return query with
            {
                Offset = offset,
                Limit = query.Limit
            };
        }
        else
        {
            return query with
            {
                Offset = offset,
                Limit = query.Limit - skip
            };
            
        }
    }
    
}