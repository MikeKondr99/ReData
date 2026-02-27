using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;

namespace ReData.DemoApp.Endpoints.Datasets.GetAll;

/// <summary>
/// Получить все наборы
/// </summary>
/// <remarks>
/// Возвращает список всех наборов данных
/// </remarks>
public class GetAllDatasetsEndpoint : EndpointWithoutRequest<Ok<List<DataSetListItem>>>
{
    public required IDatasetRepository Datasets { get; init; }

    /// <inheritdoc />
    public override void Configure()
    {
        Get("/");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    public override async Task<Ok<List<DataSetListItem>>> ExecuteAsync(CancellationToken ct)
    {
        var response = await Datasets.GetAllAsync(ct);
        return TypedResults.Ok(response);
    }
}
