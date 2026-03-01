using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Datasets.Create;

using Response =
    Results<
        Created<CreateDataSetResponse>,
        BadRequest<ErrorResponse>,
        Conflict<ErrorResponse>
    >;

/// <summary>
/// Создать набор данных
/// </summary>
/// <remarks>
/// Создаёт набор данных
/// </remarks>
public class CreateDatasetEndpoint : Endpoint<CreateDataSetRequest, Response>
{
    public required IDatasetRepository Datasets { get; init; }

    public override void Configure()
    {
        Post("/");
        Group<DataSetsGroup>();
        Summary(summary =>
        {
            summary.ExampleRequest = new CreateDataSetRequest
            {
                Name = "Имя набора",
                ConnectorId = Guid.Empty,
                Transformations =
                [
                    new TransformationBlock
                    {
                        Enabled = true,
                        Transformation = new SelectTransformation
                        {
                            Items =
                            [
                                new SelectItem
                                {
                                    Field = "Поле",
                                    Expression = "id * 2",
                                },
                            ],
                            RestOptions = SelectRestOptions.Delete,
                        },
                    },
                ],
            };
        });
        AllowAnonymous();
    }

    public override async Task<Response> ExecuteAsync(CreateDataSetRequest req, CancellationToken ct)
    {
        var entity = await Datasets.CreateAsync(
            new CreateDatasetData
            {
                Name = req.Name,
                ConnectorId = req.ConnectorId,
                Transformations = req.Transformations,
            },
            ct);

        var response = new CreateDataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        return TypedResults.Created($"/api/datasets/{entity.Id}", response);
    }
}
