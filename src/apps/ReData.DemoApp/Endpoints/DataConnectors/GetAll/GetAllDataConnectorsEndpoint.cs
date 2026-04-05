using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Groups;

namespace ReData.DemoApp.Endpoints.DataConnectors.GetAll;

/// <summary>
/// Получить все коннекторы данных
/// </summary>
/// <remarks>
/// Возвращает список всех коннекторов данных
/// </remarks>
public class GetAllDataConnectorsEndpoint : EndpointWithoutRequest<Ok<List<DataConnectorListItem>>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    /// <inheritdoc />
    public override void Configure()
    {
        Get("/");
        Group<DataConnectorsGroup>();
        AllowAnonymous();
        
        Options(x => x.CacheOutput(p => p
            .Expire(TimeSpan.FromMinutes(10))
            .Tag("data-connectors")
        ));
    }

    public override async Task<Ok<List<DataConnectorListItem>>> ExecuteAsync(
        CancellationToken ct)
    {
        var response = await Db.DataConnectors
            .OrderByDescending(ds => ds.CreatedAt)
            .Select(dc => new DataConnectorListItem()
            {
                Id = dc.Id,
                Name = dc.Name,
            })
            .ToListAsync(ct);

        return TypedResults.Ok(response);
    }
}