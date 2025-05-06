using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ReData.DemoApplication;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Functions;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ValueConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var factory = new Factory();

var connection = Environment.GetEnvironmentVariable("CONNECTION_STRING"); //"User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=pix_bi_test;Pooling=true;";
var applications = QueryBuilder.FromTable(
    factory.CreateExpressionResolver(DatabaseType.PostgreSql),
    ["TEST_DATA"],
    [
        ("ID",new FieldType(DataType.Integer, false)),
        ("CUSTOMER_NAME",new FieldType(DataType.Text, true)),
        ("EMAIL",new FieldType(DataType.Text, true)),
        ("AGE",new FieldType(DataType.Unknown, true)),
        ("ACCOUNT_BALANCE",new FieldType(DataType.Number, true)),
        ("IS_ACTIVE",new FieldType(DataType.Bool, true)),
        ("SIGNUP_DATE",new FieldType(DataType.DateTime, true)),
        ("LAST_LOGIN",new FieldType(DataType.DateTime, true)),
        ("CUSTOMER_CATEGORY",new FieldType(DataType.Text, true)),
        ("RANDOM_NUMBER",new FieldType(DataType.Integer, true)),
        ("NOTES",new FieldType(DataType.Text, true)),
        ("PURCHASE_COUNT",new FieldType(DataType.Integer, true)),
    ]
);
var compiler = factory.CreateQueryCompiler(DatabaseType.PostgreSql);

app.MapPost("api/transform", async ([FromBody] TransformRequest request) =>
    {
        string? sql = null;
        int i = -1;
        try
        {
            var query = applications;
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
                    return Results.BadRequest(new
                    {
                        index = i,
                        errors = err,
                    });
                }
            }

            await using var runner = factory.CreateQueryRunner(DatabaseType.PostgreSql, connection);
            var q = query.Build();
            // for demo debug purpose
            sql = compiler.Compile(q);
            var data = await runner.RunQueryAsObjectAsync(q);
            return Results.Ok(new TransformResponse()
            {
                Data = data,
                Query = sql.Split("\n"),
                Fields = q.Fields().Fields.Select(f => new TransformField()
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
            return Results.BadRequest(new
            {
                index = i,
                message = ex.Message,
                query = sql?.Split("\n")?.ToArray()
            });
        }
    })
    .WithName("TransformData");

app.MapGet("api/functions", () =>
{
    var functions = GlobalFunctionsStorage.Functions
        .Where(f => f.Templates.Keys.Any(k => k.HasFlag(DatabaseTypeFlags.PostgreSql)))
        .Where(f => f.ImplicitCast is null);


    return functions.Select(f => new FunctionViewModel()
    {
        Name = f.Name,
        Arguments = f.Arguments,
        Doc = f.Doc,
        Kind = f.Kind,
        ReturnType = f.ReturnType,
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
}
