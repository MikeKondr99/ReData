using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

using Response =
    Results<
        Created<CreateDataSetResponse>,
        BadRequest<ErrorResponse>,
        Conflict<ErrorResponse>
    >;

public class CreateEndpoint : Endpoint<CreateDataSetRequest, Response>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Post("/api/datasets");
        AllowAnonymous();
    }

    public override async Task<Response> ExecuteAsync(
        CreateDataSetRequest req, CancellationToken ct)
    {
        var existingDataset = Db.DataSets.FirstOrDefault(ds => ds.Name == req.Name);
        if (existingDataset is not null)
        {
            return TypedResults.Conflict(new ErrorResponse([
                new("name", "'name' must be unique")
            ]));
        }

        var newId = Guid.NewGuid();
        // Map to entity
        var entity = new DataSetEntity
        {
            Id = newId,
            Name = req.Name,
            DataConnectorId = req.ConnectorId,
            Transformations = req.Transformations.Select((t, index) => new TransformationEntity
            {
                Enabled = t.Enabled,
                Description = null,
                DataSetId = newId,
                Order = (uint)index,
                Data = t.Transformation
            }).ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };


        Db.DataSets.Add(entity);
        await Db.SaveChangesAsync(ct);

        var response = new CreateDataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        await OutputCache.EvictByTagAsync("datasets", ct);
        return TypedResults.Created($"/api/datasets/{entity.Id}", response);
    }
}