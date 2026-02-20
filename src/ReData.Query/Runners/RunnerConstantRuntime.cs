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

public sealed class RunnerConstantRuntime : IConstantRuntime
{
    private readonly IQueryRunner queryRunner;
    private readonly DbConnection connection;

    public RunnerConstantRuntime(IQueryRunner queryRunner, DbConnection connection)
    {
        this.queryRunner = queryRunner;
        this.connection = connection;
    }

    public QueryConstant Create(string name, QueryModel query, ResolvedExpr resolvedExpr)
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
        using var constantSpan = Tracing.Source.StartActivity("ConstantResolve");
        constantSpan?.SetTag("constant.name", constant.Name);
        constantSpan?.SetTag("constant.cached_before", constant.Value is not null);

        if (constant.Value is not null)
        {
            constantSpan?.SetTag("constant.resolve_mode", "value");
            return Result.Ok(constant.Value);
        }

        if (constant.Query is null)
        {
            constantSpan?.SetStatus(ActivityStatusCode.Error);
            return $"Переменная '{constant.Name}' не может быть вычислена в текущем контексте";
        }

        try
        {
            constantSpan?.SetTag("constant.resolve_mode", "computed");

            var value = queryRunner
                .GetDataReaderAsync(constant.Query, connection)
                .CollectToScalar()
                .GetAwaiter()
                .GetResult();

            constant.Value = value;
            return Result.Ok(value);
        }
        catch (Exception ex)
        {
            constantSpan?.SetStatus(ActivityStatusCode.Error);
            constantSpan?.SetTag("error", ex.Message);
            return $"Ошибка вычисления переменной '{constant.Name}': {ex.Message}";
        }
    }

    private static QueryModel BuildScalarQuery(QueryModel query, ResolvedExpr expr)
    {
        return new QueryModel
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

    private static QueryModel BuildConstScalarQuery(ResolvedExpr expr)
    {
        return new QueryModel
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
