using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Converters;
using ReData.DemoApp.Database;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Services;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Types;
using Scalar.AspNetCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;

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

services.AddFastEndpoints();
services.SwaggerDocument(options =>
{
    options.ShortSchemaNames = true;
    // Для работы требуется что бы базой был класс или абстрактный класс
    options.UseOneOfForPolymorphism = true;
    options.ExcludeNonFastEndpoints = true;
});

services.AddTickerQ(options =>
{
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

services.AddDbContext<ApplicationDatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReData"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

services.AddScoped<DwhService>();

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
app.UseFastEndpoints();

app.UseSwaggerGen(options => { options.Path = "/openapi/{documentName}.json"; });
app.UseTickerQ();

if (builder.Environment.IsDevelopment())
{
    app.MapScalarApiReference("api/docs");
}

app.Run();