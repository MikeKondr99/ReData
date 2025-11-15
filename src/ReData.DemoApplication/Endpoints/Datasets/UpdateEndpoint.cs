// Endpoints/DataSets/UpdateEndpoint.cs

using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class UpdateEndpoint : Endpoint<UpdateDataSetRequest, Results<Ok<DataSetResponse>, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Put("/api/datasets/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<DataSetResponse>, NotFound>> ExecuteAsync(
        UpdateDataSetRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("Id");

        var entity = await Db.Set<DataSetEntity>()
            .Include(ds => ds.Transformations)
            .FirstOrDefaultAsync(ds => ds.Id == id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        // Update simple properties
        entity.Name = req.Name;

        // Remove existing transformations
        Db.Set<TransformationEntity>().RemoveRange(entity.Transformations);

        // Clear and add new transformations
        entity.Transformations.Clear();

        for (int i = 0; i < req.Transformations.Count; i++)
        {
            entity.Transformations.Add(new TransformationEntity
            {
                Enabled = req.Transformations[i].Enabled,
                Description = null,
                DataSetId = id,
                Order = (uint)i,
                Data = req.Transformations[i].Transformation,
            });
        }

        await Db.SaveChangesAsync(ct);

        var response = new DataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations
                .OrderBy(t => t.Order)
                .Select(t => new TransformationBlockResponse
                {
                    Enabled = t.Enabled,
                    Description = t.Description,
                    Transformation = t.Data
                })
                .ToList()
        };

        await OutputCache.EvictByTagAsync("datasets", ct);
        return TypedResults.Ok(response);
    }
}