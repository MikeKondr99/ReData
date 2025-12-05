using System.Diagnostics;
using OpenTelemetry.Trace;
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
    Results<Ok<TransformResponse>, BadRequest<object>, InternalServerError<ExecutionErrorResponse>>>
{
    public required DwhService DwhService { get; init; }
    public IQueryCompiler Compiler { get; } = Factory.CreateQueryCompiler(DatabaseType.PostgreSql);

    public override void Configure()
    {
        Post("/api/transform");
        AllowAnonymous();
    }

    public override async
        Task<Results<Ok<TransformResponse>, BadRequest<object>, InternalServerError<ExecutionErrorResponse>>>
        ExecuteAsync(
            TransformRequest req, CancellationToken ct)
    {
        string? sql = null;
        int i = -1;
        var query = DwhService.GetQueryBuilder(req.DataConnectorId);
        Query.Core.Query build;

        // 1. Apply transformations
        using (var apply = Tracing.ReData.StartActivity("apply transformations"))
        {
            try
            {
                foreach (var transformation in req.Transformations)
                {
                    using var apply1 = Tracing.ReData.StartActivity($"apply {transformation.GetType().Name[..^14]}");
                    i++;
                    var res = transformation.Apply(query);
                    if (res.Unwrap(out var ok, out var err))
                    {
                        query = ok;
                    }
                    else
                    {
                        var errorResponse = new TransformationErrorResponse
                        {
                            Index = i,
                            Errors = err
                        };
                        return TypedResults.BadRequest((object)errorResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                apply.AddException(ex);
                apply.SetStatus(ActivityStatusCode.Error);
                var errorResponse = new CompilationErrorResponse
                {
                    Index = i,
                    Message = $"Непредвиденная ошибка при составлении создании трансформаций:\n{ex.Message}",
                    Query = sql,
                };
                return TypedResults.BadRequest((object)errorResponse);
            }
        }

        // 2. Compile SQL
        try
        {
            build = query.Build();
            sql = Compiler.Compile(build);
        }
        catch (Exception ex)
        {
            var errorResponse = new CompilationErrorResponse
            {
                Index = i,
                Message = $"Непредвиденная ошибка компиляции запроса:\n{ex.Message}",
                Query = sql,
            };
            return TypedResults.BadRequest((object)errorResponse);
        }

        using (var execute = Tracing.ReData.StartActivity("execute"))
        {
            // 3. Execute query
            try
            {
                await using var runner = Factory.CreateQueryRunner(DatabaseType.PostgreSql, DwhService.ReadConnection);
                var data = await runner.RunQueryAsObjectAsync(build);

                var response = new TransformResponse
                {
                    Data = data,
                    Query = sql,
                    Fields = build.Fields().Select(f => new TransformFieldViewModel
                    {
                        Alias = f.Alias,
                        Type = f.Type.Type,
                        CanBeNull = f.Type.CanBeNull
                    }).ToList(),
                    Total = data.Count
                };

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                execute.AddException(ex);
                execute.SetStatus(ActivityStatusCode.Error);
                var errorResponse = new ExecutionErrorResponse
                {
                    Index = i,
                    Message = $"Непредвиденная ошибка при запуске запроса:\r\n{ex.Message}",
                    Query = sql
                };
                return TypedResults.InternalServerError(errorResponse);
            }
        }
    }
}