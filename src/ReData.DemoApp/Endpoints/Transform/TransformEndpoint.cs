using ReData.DemoApp.Commands;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;
using ReData.Query.Lang.Expressions;
using Tracing = ReData.DemoApp.Extensions.Tracing;

namespace ReData.DemoApp.Endpoints.Transform;

using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Factory = ReData.Query.Factory;


/// <summary>
/// Трансформации
/// </summary>
/// <remarks>
/// Выполняет трансформации от заданного коннектора
/// И выдает постранично полученные данные
/// </remarks>
public class TransformEndpoint : Endpoint<
    TransformRequest,
    Results<Ok<TransformResponse>, BadRequest<TransformErrorResponse>, InternalServerError<TransformErrorResponse>>>
{

    public override void Configure()
    {
        Post("/transform");
        Tags("Transform");
        AllowAnonymous();
    }

    /// <inheritdoc />
    public override async
        Task<Results<Ok<TransformResponse>, BadRequest<TransformErrorResponse>,
            InternalServerError<TransformErrorResponse>>>
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



        long? total = null;
        using (var span = Tracing.ReData.StartActivity("transform-page-preparation"))
        {
            var totalQuery = query.Select(new()
            {
                ["TOTAL"] = "COUNT()",
            });

            total = (await new ExecuteQueryCommand()
            {
                Query = totalQuery.Unwrap().Build(),
            }.ExecuteAsync(ct)).Unwrap().Data[0].Int("TOTAL");

            // 2.1 Add Sort
            if (req.OrderByName is not null && req.OrderByDescending is not null)
            {
                var sort = query.OrderBy([
                    (Expr.Field(req.OrderByName), req.OrderByDescending is true ? OrderItem.Type.Desc : OrderItem.Type.Asc)
                ]);

                query = sort.UnwrapOr(query);
            }
            
            // 2. Add Paging


            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);
            
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

        ok = ok with
        {
            Total = total
        };
        return TypedResults.Ok(ok);
    }
}