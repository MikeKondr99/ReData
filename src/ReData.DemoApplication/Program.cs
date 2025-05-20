using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReData.Database;
using ReData.DemoApplication;
using ReData.DemoApplication.Extensions;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Functions;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

builder.Services.AddDbContext<ApplicationDatabaseContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));


builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ValueConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddSingleton<ConnectionService>();

var app = builder.Build();

app.Migrate<ApplicationDatabaseContext>();

app.UseDefaultFiles();
app.UseStaticFiles();

var factory = new Factory();

var compiler = factory.CreateQueryCompiler(DatabaseType.PostgreSql);

app.MapPost("api/dataset", async (
    [FromServices] DataSetsService datasets
) =>
{
    await datasets.CreateDataSetAsync(new DataSet()
    {
        Id = Guid.NewGuid(),
        Name = $"Набор {Guid.NewGuid().ToString("N")[..6]}",
        Transformations =
        [
            new ExtractTransformation(),
        ]
    });
});

app.MapPut("api/dataset", async (
    DataSet dataSet,
    [FromServices] DataSetsService datasets,
    CancellationToken cancellationToken
) =>
{
    return await datasets.UpdateDataSetAsync(dataSet, cancellationToken);
});

app.MapDelete("api/dataset/:id", async (
    [FromQuery] Guid id,
    [FromServices] DataSetsService datasets,
    CancellationToken cancellationToken
) =>
{
    await datasets.DeleteDataSetAsync(id);
});

app.MapGet("api/dataset", async (
    [FromServices] DataSetsService datasets,
    CancellationToken cancellationToken
) =>
{
    return await datasets.GetAllDataSetsAsync(cancellationToken);
});

app.MapGet("api/dataset/:id", async (
    [FromQuery] Guid id,
    [FromServices] DataSetsService datasets,
    CancellationToken cancellationToken
) =>
{
    return await datasets.GetDataSetAsync(id);
});

app.MapPost("api/dataset/preview", async ([FromBody] TransformRequest request, 
        [FromServices] IServiceProvider services,
        [FromServices] ConnectionService connectionService) =>
    {
        string? sql = null;
        int i = -1;
        QueryBuilder query = null!;
        Query build;
        // Сбор трансформаций
        try
        {
            foreach (var transformation in request.Transformations)
            {
                i++;
                var res = transformation.Apply(query, services);
                if (res.Unwrap(out var ok, out var err))
                {
                    query = ok;
                }
                else
                {
                    return Results.BadRequest(new
                    {
                        index = i,
                        errors = err,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(new
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
            return Results.InternalServerError(new
            {
                index = i,
                message = $"Непредвиденная ошибка компиляции запроса:\n{ex.Message}",
                query = sql?.Split("\n")?.ToArray()
            });
        }

        // Запуск запроса
        try
        {
            await using var runner = factory.CreateQueryRunner(DatabaseType.PostgreSql, connectionService.Connection);
            var data = await runner.RunQueryAsObjectAsync(build);
            
            return Results.Ok(new TransformResponse()
                {
                    Data = data,
                    Query = sql,
                    Fields = build.Fields().Fields.Select(f => new TransformField()
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
            return Results.InternalServerError(new
            {
                index = i,
                message = $"Непредвиденная ошибка при запуске запроса:\r\n{ex.Message}",
                query = sql
            });
        }
    })
    .WithName("TransformData");

app.MapGet("api/functions", () =>
{
    var functions = GlobalFunctionsStorage.Functions
        .Where(f => f.Templates.Keys.Any(k => k.HasFlag(DatabaseTypeFlags.PostgreSql)))
        .Where(f => f.ImplicitCast is null)
        .OrderBy(f => f.Name);


    return functions.Select(f => new FunctionViewModel()
    {
        Name = f.Name,
        Arguments = f.Arguments,
        Doc = f.Doc,
        Kind = f.Kind,
        ReturnType = f.ReturnType,
        DisplayText = f.ToString(),
    });
});

app.Run();


public sealed record FunctionViewModel
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }
    
    public required string DisplayText { get; init; }
}
