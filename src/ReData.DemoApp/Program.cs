using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ReData.DemoApp;
using ReData.DemoApp.CommandMiddleware;
using ReData.DemoApp.Converters;
using ReData.DemoApp.Database;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Services;
using ReData.DemoApp.Transformations;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using Scalar.AspNetCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Instrumentation.OpenTelemetry;
using Factory = ReData.Query.Factory;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var tickerQConnectionString = builder.Configuration.GetConnectionString("TickerQ");

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
});

services.AddOutputCache();

services.AddTickerQ(options =>
{
    options.AddOpenTelemetryInstrumentation();
    options.ConfigureScheduler(scheduler =>
    {
        scheduler.MaxConcurrency = 8;
        scheduler.NodeIdentifier = "main-server";
    });
    options.AddOperationalStore(efOptions =>
    {
        // Use built-in TickerQDbContext with connection string
        efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
        {
            // dotnet ef migrations add TickerQInitialCreate --context TickerQDbContext --project ./src/ReData.DemoApp -o ./Jobs/Migrations
            optionsBuilder.UseNpgsql(tickerQConnectionString,
                builder =>
                {
                    builder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), ["40P01"]);
                    builder.MigrationsAssembly("ReData.DemoApp");
                });
        });
    });
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/api/tickerq");
        dashboardOptions.WithBasicAuth("test", "secret9");
    });
});

services.AddFastEndpoints();



services.SwaggerDocument(options =>
{
    options.ShortSchemaNames = true;
    options.AutoTagPathSegmentIndex = 0;
    options.DocumentSettings = (settings) =>
    {
        settings.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
        settings.SchemaSettings.SchemaProcessors.Add(new RequiredPropertiesSchemaProcessor());
        // settings.OperationProcessors.Add(new SimplifyOperationIdProcessor());
        settings.PostProcess = document =>
        {
            document.Host = "HOST";

        };
    };
    // Для работы требуется что бы базой был класс или абстрактный класс
    options.UseOneOfForPolymorphism = true;
    options.ExcludeNonFastEndpoints = true;
});


services.AddDbContext<ApplicationDatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReData"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Net.Http");
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("TickerQ")
            .AddSource("ReData.App")
            .AddSource("ReData.Query.Lang")
            .AddSource("ReData.Query")
            .AddHttpClientInstrumentation(options => { options.RecordException = true; })
            .AddAspNetCoreInstrumentation(options =>
            {
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    if (request.Cookies.TryGetValue("visitorId", out var visitorId))
                    {
                        activity.SetTag("VisitorId", visitorId);
                    }
                };
                options.RecordException = true;
                options.Filter = (httpContext) =>
                {
                    return httpContext.Request.Path.StartsWithSegments("/api");
                };
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.Filter = (_, dbCommand) =>
                {
                    return dbCommand.Connection.Database != "TickerQ";
                };
            });
    })
    .UseOtlpExporter();

services.AddScoped<DwhService>();

services.AddCommandMiddleware(c =>
{
    c.Register(typeof(TraceCommandMiddleware<,>));
});


var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();

    // If there's no available file and the request doesn't start with /api
    if (context.Response.StatusCode == 404 &&
        context.Request.Path.Value?.StartsWith("/api", StringComparison.Ordinal) != true)
    {
        context.Request.Path = "/index.html";
        await next();
    }
});

app.Services.Migrate<ApplicationDatabaseContext>();
app.Services.Migrate<TickerQDbContext>();

app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseFastEndpoints(c =>
{
    c.Endpoints.ShortNames = true;
    c.Endpoints.RoutePrefix = "api";
    c.Endpoints.NameGenerator = (context) =>
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

app.UseTickerQ();

if (builder.Environment.IsDevelopment())
{
    app.MapScalarApiReference("api/docs");
}

app.Run();