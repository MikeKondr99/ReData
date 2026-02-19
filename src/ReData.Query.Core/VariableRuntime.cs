using Pattern.Unions;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;
using SqlTemplate = ReData.Query.Core.Template.Template;

namespace ReData.Query.Core;

public sealed record QueryVariable
{
    public required string Name { get; init; }

    public Query? Query { get; init; }

    public IValue? Value { get; set; }

    public static QueryVariable FromValue(string name, IValue value)
    {
        return new QueryVariable
        {
            Name = name,
            Value = value,
        };
    }
}

public interface IVariableRuntime
{
    QueryVariable Create(string name, Query query, ResolvedExpr resolvedExpr);

    Result<IValue, string> Resolve(QueryVariable variable);
}

public sealed class DisabledVariableRuntime : IVariableRuntime
{
    public static readonly DisabledVariableRuntime Instance = new();

    private DisabledVariableRuntime()
    {
    }

    public QueryVariable Create(string name, Query query, ResolvedExpr resolvedExpr)
    {
        var scalarQuery = resolvedExpr.Type is { IsConstant: true, Aggregated: false }
            ? BuildConstScalarQuery(resolvedExpr)
            : BuildScalarQuery(query, resolvedExpr);

        return new QueryVariable
        {
            Name = name,
            Query = scalarQuery,
            Value = null,
        };
    }

    public Result<IValue, string> Resolve(QueryVariable variable)
    {
        if (variable.Value is not null)
        {
            return Result.Ok(variable.Value);
        }

        return "Вычисление сложных переменных выключено в данном контексте";
    }

    private static Query BuildScalarQuery(Query query, ResolvedExpr expr)
    {
        return new Query
        {
            Name = new ResolvedTemplate(SqlTemplate.Create("VariableRuntimeQuery")),
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
            Name = new ResolvedTemplate(SqlTemplate.Create("VariableRuntimeConstQuery")),
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
