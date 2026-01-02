using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Groups;

namespace ReData.DemoApp.Endpoints.Datasets.GetAll;

/// <summary>
/// Получить все наборы
/// </summary>
/// <remarks>
/// Возвращает список всех наборов данных
/// </remarks>
public class GetAllDatasetsEndpoint : EndpointWithoutRequest<
    Ok<List<DataSetListItem>>
>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public override void Configure()
    {
        Get("/");
        Group<DataSetsGroup>();
        // Get("/api/datasets");
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