// Endpoints/DataSets/DeleteEndpoint.cs

using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class DeleteEndpoint : Endpoint<DeleteDataSetRequest, Results<Ok, NotFound>>
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
        var entity = await Db.Set<DataSetEntity>()
            .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        Db.Set<DataSetEntity>().Remove(entity);
        await Db.SaveChangesAsync(ct);
        await OutputCache.EvictByTagAsync("datasets", ct);

        return TypedResults.Ok();
    }
}