using System.Data.Common;
using System.Diagnostics;
using Pattern.Unions;
using ReData.Query.Core;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;
using QueryModel = ReData.Query.Core.Query;
using SqlTemplate = ReData.Query.Core.Template.Template;

namespace ReData.Query.Runners;

public sealed class RunnerVariableRuntime : IVariableRuntime
{
    private readonly IQueryRunner queryRunner;
    private readonly DbConnection connection;

    public RunnerVariableRuntime(IQueryRunner queryRunner, DbConnection connection)
    {
        this.queryRunner = queryRunner;
        this.connection = connection;
    }

    public QueryVariable Create(string name, QueryModel query, ResolvedExpr resolvedExpr)
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
        using var variableSpan = Tracing.Source.StartActivity("VariableRuntimeResolve");
        variableSpan?.SetTag("variable.name", variable.Name);
        variableSpan?.SetTag("variable.cached_before", variable.Value is not null);

        if (variable.Value is not null)
        {
            variableSpan?.SetTag("variable.resolve_mode", "value");
            return Result.Ok(variable.Value);
        }

        if (variable.Query is null)
        {
            variableSpan?.SetStatus(ActivityStatusCode.Error);
            return $"Переменная '{variable.Name}' не может быть вычислена в текущем контексте";
        }

        try
        {
            variableSpan?.SetTag("variable.resolve_mode", "computed");
            variableSpan?.SetTag("variable.query_name", variable.Query.Name.Template.ToString());

            var value = queryRunner
                .GetDataReaderAsync(variable.Query, connection)
                .CollectToScalar()
                .GetAwaiter()
                .GetResult();

            variable.Value = value;
            return Result.Ok(value);
        }
        catch (Exception ex)
        {
            variableSpan?.SetStatus(ActivityStatusCode.Error);
            variableSpan?.SetTag("error", ex.Message);
            return $"Ошибка вычисления переменной '{variable.Name}': {ex.Message}";
        }
    }

    private static QueryModel BuildScalarQuery(QueryModel query, ResolvedExpr expr)
    {
        return new QueryModel
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

    private static QueryModel BuildConstScalarQuery(ResolvedExpr expr)
    {
        return new QueryModel
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
