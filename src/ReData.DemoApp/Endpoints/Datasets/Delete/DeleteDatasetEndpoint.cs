using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Groups;

namespace ReData.DemoApp.Endpoints.Datasets.Delete;

/// <summary>
/// Удалить набор данных
/// </summary>
/// <remarks>
/// Удаляет набор данных с указанным id
/// </remarks>
public class DeleteDatasetEndpoint : Endpoint<DeleteDataSetRequest, Results<Ok, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required IOutputCacheStore OutputCache { get; init; }


    public override void Configure()
    {
        Delete("/{Id}");
        Group<DataSetsGroup>();
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