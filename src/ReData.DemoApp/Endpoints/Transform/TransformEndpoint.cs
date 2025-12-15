using System.Diagnostics;
using OpenTelemetry.Trace;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Services;
using ReData.Query;
using ReData.Query.Core.Components;

namespace ReData.DemoApp.Endpoints.Transform;

using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Factory = ReData.Query.Factory;

public class TransformEndpoint : Endpoint<
    TransformRequest,
    Results<Ok<TransformResponse>, BadRequest<TransformErrorResponse>, InternalServerError<TransformErrorResponse>>>
{
    public IQueryCompiler Compiler { get; } = Factory.CreateQueryCompiler(DatabaseType.PostgreSql);

    public override void Configure()
    {
        Post("/api/transform");
        AllowAnonymous();
    }

    /// <inheritdoc />
    public override async
        Task<Results<Ok<TransformResponse>, BadRequest<TransformErrorResponse>, InternalServerError<TransformErrorResponse>>>
        ExecuteAsync(
            TransformRequest req,
            CancellationToken ct)
    {
        // 1. Apply transformations
        var res1 = await new ApplyTransformationsCommand()
        {
            DataConnectorId = req.DataConnectorId,
            Transformations = req.Transformations,
        }.ExecuteAsync(ct);

        if (res1.UnwrapErr(out var err1, out var query))
        {
            return TypedResults.BadRequest(new TransformErrorResponse()
            {
                Index = err1.Index,
                Errors = err1.Errors!,
                Message = err1.Message
            });
        }

        // 2. Compile SQL
        try
        {
            var sql = Compiler.Compile(query.Build());
        }
        catch (Exception ex)
        {
            var errorResponse = new TransformErrorResponse()
            {
                Index = req.Transformations.Count,
                Message = $"Непредвиденная ошибка компиляции запроса:\n{ex.Message}",
                Errors = null,
            };
            return TypedResults.BadRequest(errorResponse);
        }

        // 3. Query execution
        var execRes = await new ExecuteQueryCommand()
        {
            Query = query.Build(),
        }.ExecuteAsync(ct);

        if (execRes.UnwrapErr(out string err, out var ok))
        {
            return TypedResults.InternalServerError(new TransformErrorResponse()
            {
                Message = err,
                Index = req.Transformations.Count,
                Errors = null,
            });
        }

        return TypedResults.Ok(ok);
    }
}