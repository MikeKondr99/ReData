using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class GetAllEndpoint : EndpointWithoutRequest<
    Ok<List<DataSetListItem>>
>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public override void Configure()
    {
        Get("/api/datasets");
        AllowAnonymous();
        
        Options(x => x.CacheOutput(p => p
            .Expire(TimeSpan.FromMinutes(10))
            .Tag("datasets")
        ));
    }

    public override async Task<Ok<List<DataSetListItem>>> ExecuteAsync(
        CancellationToken ct)
    {
        var response = await Db.DataSets
            .OrderByDescending(ds => ds.UpdatedAt)
            .Select(ds => new DataSetListItem()
            {
                Id = ds.Id,
                Name = ds.Name,
            })
            .ToListAsync(ct);

        return TypedResults.Ok(response);
    }
}