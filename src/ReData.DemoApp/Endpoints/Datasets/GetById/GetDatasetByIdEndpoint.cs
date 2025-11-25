using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.GetById;

namespace ReData.DemoApp.Endpoints.Datasets;

using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

public class GetDatasetByIdEndpoint : Endpoint<GetByIdRequest, Results<Ok<DataSetResponse>, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public override void Configure()
    {
        Get("/api/datasets/{Id}");
        AllowAnonymous();
        
        Options(x => x.CacheOutput(p => p
            .Expire(TimeSpan.FromMinutes(10))
            .Tag("datasets")
        ));
    }

    public override async Task<Results<Ok<DataSetResponse>, NotFound>> ExecuteAsync(
        GetByIdRequest req, CancellationToken ct)
    {
        var entity = await Db.Set<DataSetEntity>()
            .Include(ds => ds.Transformations)
            .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        var response = new DataSetResponse
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

        return TypedResults.Ok(response);
    }
}