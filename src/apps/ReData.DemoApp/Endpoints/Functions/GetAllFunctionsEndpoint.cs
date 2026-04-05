using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;
using ReData.Query.Impl.Functions;

namespace ReData.DemoApp.Endpoints.Functions;

/// <summary>
/// Получить все функции
/// </summary>
/// <remarks>
/// Возвращает полный список доступных для провайдера PostgreSql функций
/// </remarks>
public class GetAllFunctionsEndpoint : EndpointWithoutRequest<
    Results<
        Ok<List<FunctionResponse>>,
        NotFound,
        ProblemDetails
    >>
{
    public override void Configure()
    {
        Get("/functions");
        Tags("Functions");
        AllowAnonymous();

        Options(x => x.CacheOutput(p =>
            p.Expire(TimeSpan.FromDays(1))
        ));
    }


    /// <inheritdoc />
    public override async Task<Results<Ok<List<FunctionResponse>>, NotFound, ProblemDetails>> ExecuteAsync(
        CancellationToken ct)
    {
        var functions = GlobalFunctionsStorage.Functions
            .Where(f => f.Templates.Keys.Any(k => k.HasFlag(DatabaseTypes.PostgreSql)))
            .Where(f => f.ImplicitCast is null)
            .OrderBy(f => f.Name)
            .Select(f => new FunctionResponse()
            {
                Name = f.Name,
                Arguments = f.Arguments,
                Doc = f.Doc,
                Kind = f.Kind,
                ReturnType = f.ReturnType,
                DisplayText = f.ToString(),
            })
            .ToList();

        if (functions.Count == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(functions);
    }
}