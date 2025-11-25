using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetById;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

public class UpdateDatasetEndpoint : Endpoint<UpdateDataSetRequest, Results<Ok<UpdateDataSetResponse>, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Put("/api/datasets/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UpdateDataSetResponse>, NotFound>> ExecuteAsync(
        UpdateDataSetRequest req, CancellationToken ct)
    {
        var entity = await Db.Set<DataSetEntity>()
            .AsTracking()
            .Include(ds => ds.Transformations)
            .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        // Update simple properties
        entity.Name = req.Name;
        entity.DataConnectorId = req.ConnectorId;

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
                DataSetId = req.Id,
                Order = (uint)i,
                Data = req.Transformations[i].Transformation,
            });
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await Db.SaveChangesAsync(ct);

        var response = new UpdateDataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            DataConnectorId = entity.DataConnectorId,
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