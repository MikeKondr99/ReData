using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;

namespace ReData.DemoApp.Endpoints.Datasets.Delete;

/// <summary>
/// Удалить набор данных
/// </summary>
/// <remarks>
/// Удаляет набор данных с указанным id
/// </remarks>
public class DeleteDatasetEndpoint : Endpoint<DeleteDataSetRequest, Results<Ok, NotFound>>
{
    public required IDatasetRepository Datasets { get; init; }

    public override void Configure()
    {
        Delete("/{Id}");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    public override async Task<Results<Ok, NotFound>> ExecuteAsync(DeleteDataSetRequest req, CancellationToken ct)
    {
        var deleted = await Datasets.DeleteAsync(req.Id, ct);
        return deleted ? TypedResults.Ok() : TypedResults.NotFound();
    }
}
