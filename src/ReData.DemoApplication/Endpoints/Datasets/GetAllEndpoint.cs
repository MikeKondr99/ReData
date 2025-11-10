using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class GetAllEndpoint : EndpointWithoutRequest<
    Ok<List<DataSetResponse>>
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

    public override async Task<Ok<List<DataSetResponse>>> ExecuteAsync(
        CancellationToken ct)
    {
        var datasetEntities = await Db.Set<DataSetEntity>()
            .Include(ds => ds.Transformations)
            .OrderBy(ds => ds.Name)
            .ToListAsync(ct);

        var response = datasetEntities.Select(entity => new DataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations
                .OrderBy(t => t.Order)
                .Select(t => new TransformationBlockResponse
                {
                    Enabled = t.Enabled,
                    Description = t.Description,
                    Transformation = t.Data,
                })
                .ToList()
        }).ToList();

        return TypedResults.Ok(response);
    }
}