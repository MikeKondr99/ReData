using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using ReData.Jobs;
using ReData.DemoApp;
using ReData.DemoApp.CommandMiddleware;
using ReData.DemoApp.Converters;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Datasets.Export;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Middleware;
using ReData.DemoApp.Repositories.Datasets;
using ReData.DemoApp.Services;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Types;
using Scalar.AspNetCore;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();

services.AddAuthentication()
    .AddJwtBearer(options => {
        options.Authority = "http://localhost:8080/realms/redata";
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/redata",
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateLifetime = true,
        };
    });
services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new ValueConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<FunctionKind>());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<DataType>());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<SelectRestOptions>());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<ExportFileType>());
});

services.AddOutputCache();

builder.AddReDataJobs(ReDataJobsMode.ProducerDashboard);

services.AddFastEndpoints();

services.SwaggerDocument(options =>
{
    options.ShortSchemaNames = true;
    options.AutoTagPathSegmentIndex = 0;
    options.DocumentSettings = settings =>
    {
        settings.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
        settings.SchemaSettings.SchemaProcessors.Add(new RequiredPropertiesSchemaProcessor());
        settings.PostProcess = document =>
        {
            document.Host = "HOST";
        };
    };
    options.ExcludeNonFastEndpoints = true;
});

services.AddDbContext<ApplicationDatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReData"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

services.AddScoped<ConnectorQueryBuilderService>();
services.AddScoped<IConnectionService, ConnectionService>();
services.AddScoped<IDatasetRepository, DatasetRepository>();

services.AddCommandMiddleware(c =>
{
    c.Register(typeof(TraceCommandMiddleware<,>));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 &&
        context.Request.Path.Value?.StartsWith("/api", StringComparison.Ordinal) != true)
    {
        context.Request.Path = "/index.html";
        await next();
    }
});

app.Services.Migrate<ApplicationDatabaseContext>();
app.Services.Migrate<TickerQDbContext>();
app.MapDefaultEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ApiFailureLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Endpoints.ShortNames = true;
    c.Endpoints.RoutePrefix = "api";
    c.Endpoints.NameGenerator = context =>
    {
        var name = context.EndpointType.Name;
        if (name.EndsWith("Endpoint", StringComparison.InvariantCulture))
        {
            return name[..^8];
        }

        return name;
    };
    c.Serializer.Options.Converters.Add(new ValueConverter());
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<FunctionKind>());
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<DataType>());
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter<SelectRestOptions>());
});

app.UseSwaggerGen(options =>
{
    options.Path = "/openapi/{documentName}.json";
});

app.MapPost("/api/dev/tickerq/test-job", async (ITimeTickerManager<TimeTickerEntity> manager, CancellationToken ct) =>
{
    var executionTime = DateTime.UtcNow.AddSeconds(30);
    var ticker = new TimeTickerEntity
    {
        Function = ReDataJobsExtensions.GetTestJobFunctionName(),
        Description = "Temporary DemoApp endpoint test job",
        ExecutionTime = executionTime
    };
    var result = await manager.AddAsync(ticker, ct);

    if (!result.IsSucceeded || result.Result is null)
    {
        return Results.Problem(
            detail: result.Exception?.Message ?? "TickerQ failed to schedule TestJob.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    ticker = result.Result;

    return Results.Ok(new
    {
        message = "TestJob scheduled",
        ticker.Id,
        ticker.Function,
        ticker.Description,
        ticker.ExecutionTime
    });
});

app.UseTickerQ();
app.MapScalarApiReference("api/docs");

await app.RunAsync();

public partial class Program;
