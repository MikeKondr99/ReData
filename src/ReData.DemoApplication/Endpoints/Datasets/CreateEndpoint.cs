using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class CreateEndpoint : Endpoint<CreateDataSetRequest, Results<Created<CreateDataSetResponse>, BadRequest<string>>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Post("/api/datasets");
        AllowAnonymous();
    }

    public override async Task<Results<Created<CreateDataSetResponse>, BadRequest<string>>> ExecuteAsync(
        CreateDataSetRequest req, CancellationToken ct)
    {
        var newId = Guid.NewGuid();
        // Map to entity
        var entity = new DataSetEntity
        {
            Id = newId,
            Name = req.Name,
            TableId = req.TableId,
            FieldList = [..req.FieldList],
            Transformations = req.Transformations.Select((t, index) => new TransformationEntity
            {
                Enabled = t.Enabled,
                Description = t.Description,
                DataSetId = newId,
                Order = (uint)index,
                Data = t.Transformation
            }).ToList()
        };

        try
        {
            Db.Set<DataSetEntity>().Add(entity);
            await Db.SaveChangesAsync(ct);

            var response = new CreateDataSetResponse
            {
                Id = entity.Id,
                Name = entity.Name,
            };

            await OutputCache.EvictByTagAsync("datasets", ct);
            return TypedResults.Created($"/api/datasets/{entity.Id}", response);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbUpdateConcurrencyException)
        {
            return TypedResults.BadRequest("Failed to create dataset");
        }
    }
}
