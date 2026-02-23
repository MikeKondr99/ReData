using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Http.Extensions;
using Npgsql;
using Pattern.Unions;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Database.Migrations;
using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Services;
using ReData.DemoApp.Transformations;
using ReData.Query;
using ReData.Query.Common;
using ReData.Query.Executors;
using Factory = ReData.Query.Factory;
using QueryBuilder = ReData.Query.Core.QueryBuilder;

namespace ReData.DemoApp.Commands;

public record ApplyTransformationsCommand : ICommand<Result<QueryBuilder, ApplyTransformationError>>
{
    public required Guid DataConnectorId { get; init; }

    public required IReadOnlyList<Transformation> Transformations { get; init; }
}

public record struct ApplyTransformationError
{
    public required int Index { get; init; }
    
    public required string Message { get; init; }

    public IEnumerable<IReadOnlyList<ExprError>>? Errors { get; init; }
}

public class ApplyTransformationsCommandHandler(DwhService dwhService)
    : ICommandHandler<ApplyTransformationsCommand, Result<QueryBuilder, ApplyTransformationError>>
{
    /// <inheritdoc />
    public async Task<Result<QueryBuilder, ApplyTransformationError>> ExecuteAsync(ApplyTransformationsCommand command,
        CancellationToken ct)
    {
        var i = 0;
        try
        {
            await using var connection = new NpgsqlConnection(dwhService.ReadConnection);
            var constantRuntime = new RunnerConstantRuntime(
                Factory.CreateQueryExecuter(DatabaseType.PostgreSql),
                connection);

            var query = dwhService.GetQueryBuilder(command.DataConnectorId, constantRuntime);
            for (i = 0; i < command.Transformations.Count; i++)
            {
                var transformation = command.Transformations[i];
                if (!ApplyTransformation(transformation, ref query, out var errors))
                {
                    return new ApplyTransformationError()
                    {
                        Index = i,
                        Message = null!,
                        Errors = errors,
                    };
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            return new ApplyTransformationError()
            {
                Index = i,
                Message = $"Непредвиденная ошибка при применении трансформаций:\n{ex.Message}",
            };
        }
    }

    private static bool ApplyTransformation(
        Transformation transformation,
        ref QueryBuilder query,
        out IEnumerable<IReadOnlyList<ExprError>>? error)
    {
        using var apply1 = Tracing.ReData.StartActivity($"apply {transformation.GetType().Name[..^14]}");
        var res = transformation.Apply(query);
        if (res.Unwrap(out var ok, out var err))
        {
            query = ok;
            error = null;
            return true;
        }

        error = err;
        apply1?.SetStatus(ActivityStatusCode.Error);
        return false;
    }
}
