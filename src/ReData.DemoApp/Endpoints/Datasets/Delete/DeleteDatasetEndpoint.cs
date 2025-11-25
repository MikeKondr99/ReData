using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.DataSets;

namespace ReData.DemoApp.Endpoints.Datasets.Delete;

public class DeleteDatasetEndpoint : Endpoint<DeleteDataSetRequest, Results<Ok, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required IOutputCacheStore OutputCache { get; init; }


    public override void Configure()
    {
        Delete("/api/datasets/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok, NotFound>> ExecuteAsync(
        DeleteDataSetRequest req, CancellationToken ct)
    {
        var entity = await Db.DataSets
            .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        Db.DataSets.Remove(entity);
        await Db.SaveChangesAsync(ct);
        await OutputCache.EvictByTagAsync("datasets", ct);

        return TypedResults.Ok();
    }
}