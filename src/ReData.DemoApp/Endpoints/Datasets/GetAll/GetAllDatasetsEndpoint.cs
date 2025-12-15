using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;

namespace ReData.DemoApp.Endpoints.Datasets.GetAll;

public class GetAllDatasetsEndpoint : EndpointWithoutRequest<
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
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt,
                FieldList = ds.FieldList,
                RowsCount = ds.RowsCount,
            })
            .ToListAsync(ct);

        return TypedResults.Ok(response);
    }
}