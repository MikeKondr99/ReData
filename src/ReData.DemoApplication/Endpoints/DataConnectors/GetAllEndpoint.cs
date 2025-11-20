using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;
using ReData.DemoApplication.Endpoints.DataSets;

namespace ReData.DemoApplication.Endpoints.DataConnectors;

public class GetAllEndpoint : EndpointWithoutRequest<
    Ok<List<DataConnectorListItem>>
>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public override void Configure()
    {
        Get("/api/data-connectors");
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