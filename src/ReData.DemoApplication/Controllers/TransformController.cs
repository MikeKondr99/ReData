using Microsoft.AspNetCore.Mvc;
using ReData.DemoApplication.Requests;
using ReData.DemoApplication.Responses;
using ReData.DemoApplication.Services;
using ReData.Query;
using ReData.Query.Core.Components;

namespace ReData.DemoApplication.Controllers;

[ApiController]
[Route("api/transform")]
public class TransformController : ControllerBase
{
    private readonly ConnectionService connectionService;
    private readonly IQueryCompiler compiler = Factory.CreateQueryCompiler(DatabaseType.PostgreSql);

    public TransformController(ConnectionService connectionService)
    {
        this.connectionService = connectionService;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(TransformRequest request)
    {
        string? sql = null;
        int i = -1;
        var query = connectionService.GetQuery();
        Query.Core.Query build;
        // Сбор трансформаций
        try
        {
            foreach (var transformation in request.Transformations)
            {
                i++;
                var res = transformation.Apply(query);
                if (res.Unwrap(out var ok, out var err))
                {
                    query = ok;
                }
                else
                {
                    return BadRequest(new
                    {
                        index = i,
                        errors = err,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                index = i,
                message = $"Непредвиденная ошибка при составлении создании трансформаций:\n{ex.Message}",
                query = sql?.Split("\n")?.ToArray()
            });
        }

        // Создание Sql (Нужно для демо)
        try
        {
            // for demo debug purpose
            build = query.Build();
            sql = compiler.Compile(build);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                index = i,
                message = $"Непредвиденная ошибка компиляции запроса:\n{ex.Message}",
                query = sql?.Split("\n")?.ToArray()
            });
        }

        // Запуск запроса
        try
        {
            await using var runner =
                Factory.CreateQueryRunner(ReData.Query.DatabaseType.PostgreSql, connectionService.Connection);
            var data = await runner.RunQueryAsObjectAsync(build);

            return Ok(new TransformResponse()
            {
                Data = data,
                Query = sql,
                Fields = build.Fields().Fields.Select(f => new TransformFieldViewModel()
                {
                    Alias = f.Alias,
                    Type = f.Type.Type,
                    CanBeNull = f.Type.CanBeNull
                }).ToList(),
                Total = data.Count,
            });
        }
        catch (Exception ex)
        {
            var obj = new
            {
                index = i,
                message = $"Непредвиденная ошибка при запуске запроса:\r\n{ex.Message}",
                query = sql
            };
            return StatusCode(500, obj);
        }
    }
}