using Pattern.Unions;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;
using SqlTemplate = ReData.Query.Core.Template.Template;

namespace ReData.Query.Core;

public sealed record QueryConstant
{
    public required string Name { get; init; }

    public Query? Query { get; init; }

    public IValue? Value { get; set; }

    public static QueryConstant FromValue(string name, IValue value)
    {
        return new QueryConstant
        {
            Name = name,
            Value = value,
        };
    }
}

public interface IConstantRuntime
{
    QueryConstant Create(string name, Query query, ResolvedExpr resolvedExpr);

    Result<IValue, string> Resolve(QueryConstant constant);
}

public sealed class DisabledConstantRuntime : IConstantRuntime
{
    public static readonly DisabledConstantRuntime Instance = new();

    private DisabledConstantRuntime()
    {
    }

    public QueryConstant Create(string name, Query query, ResolvedExpr resolvedExpr)
    {
        var scalarQuery = resolvedExpr.Type is { IsConstant: true, Aggregated: false }
            ? BuildConstScalarQuery(resolvedExpr)
            : BuildScalarQuery(query, resolvedExpr);

        return new QueryConstant
        {
            Name = name,
            Query = scalarQuery,
            Value = null,
        };
    }

    public Result<IValue, string> Resolve(QueryConstant constant)
    {
        if (constant.Value is not null)
        {
            return Result.Ok(constant.Value);
        }

        return "Вычисление сложных констант выключено в данном контексте";
    }

    private static Query BuildScalarQuery(Query query, ResolvedExpr expr)
    {
        return new Query
        {
            Name = new ResolvedTemplate(SqlTemplate.Create("ConstQuery")),
            From = query,
            Select =
            [
                new SelectItem(
                    Alias: "__var",
                    Column: new ResolvedTemplate(SqlTemplate.Create("__var")),
                    ResolvedExpr: expr)
            ],
        };
    }

    private static Query BuildConstScalarQuery(ResolvedExpr expr)
    {
        return new Query
        {
            Name = new ResolvedTemplate(SqlTemplate.Create("ConstQuery")),
            Select =
            [
                new SelectItem(
                    Alias: "__var",
                    Column: new ResolvedTemplate(SqlTemplate.Create("__var")),
                    ResolvedExpr: expr)
            ],
        };
    }
}
