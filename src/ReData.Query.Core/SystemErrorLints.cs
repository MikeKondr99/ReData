using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core;

public static class SystemErrorLints
{
    public static string? NotBool(ResolvedExpr expr)
    {
        if (expr.Type.DataType is DataType.Bool)
        {
            return "Выражение не может быть булевым";
        }

        return null;
    }
    
    public static string? Bool(ResolvedExpr expr)
    {
        if (expr.Type.DataType is not DataType.Bool)
        {
            return "Выражение должно булевым";
        }

        return null;
    }

    public static string? NotConst(ResolvedExpr expr)
    {
        if (expr.Type.IsConstant)
        {
            return "Выражение не может быть константой";
        }

        return null;
    }

    public static string? NotNull(ResolvedExpr expr)
    {
        if (expr.Type.DataType is DataType.Null)
        {
            return "Выражение не может быть NULL";
        }

        return null;
    }
    
    public static string? NotAggregated(ResolvedExpr expr)
    {
        if (expr.Type.Aggregated)
        {
            return "Выражение не может быть агрегированным";
        }

        return null;
    }
    
    public static Func<ResolvedExpr,string?> AggregatedOrGrouped(IEnumerable<ResolvedExpr> grouped)
    {
        return (ResolvedExpr expr) =>
        {
            if (!expr.Type.Aggregated && !expr.Type.IsConstant &&
                !grouped.Any(g => expr.Node.Equivalent(g.Node)))
            {
                return "Выражение не может быть агрегированным";
            }

            return null;
        };
    }
}