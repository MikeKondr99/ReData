using Microsoft.AspNetCore.Mvc;

namespace ReData.Application.Controllers.DataSet;

[ApiController]
[Route("/api/dataset")]
public sealed class DataSourceController : ControllerBase
{
    public required Domain.IRepository<Domain.DataSet> Repository { private get; init; }

    [HttpGet]
    public async Task<ActionResult> Test(CancellationToken ct)
    {
        var create = await Repository.CreateAsync(new Domain.DataSet()
        {
            Id = Guid.NewGuid(),
            Name = $"Name_{Guid.NewGuid()}",
            Transformations = [
                new Entity.LoadTransformation()
                {
                    Table = "Table",
                    DataSourceId = Guid.Empty,
                    Fields =
                    [
                        new Entity.Field
                        {
                            Name = "id",
                            Type = Entity.FieldType.Integer,
                            Optional = false,
                        },
                        new Entity.Field
                        {
                            Name = "name",
                            Type = Entity.FieldType.Text,
                            Optional = false,
                        },
                        new Entity.Field
                        {
                            Name = "height",
                            Type = Entity.FieldType.Real,
                            Optional = true,
                        }
                    ]

                }
            ]
        });

        var get = await Repository.GetAsync(x => true, ct);
        return Ok();
    }

}