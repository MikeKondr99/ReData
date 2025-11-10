using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Converters;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Extensions;
using ReData.DemoApplication.Services;
using Scalar.AspNetCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var tickerQConnectinoString = builder.Configuration.GetConnectionString("TickerQ");

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new ValueConverter());
    options.SerializerOptions.Converters.Add(new DataTypeJsonConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

services.AddOutputCache();

services.AddFastEndpoints();
services.SwaggerDocument();

services.AddTickerQ(options =>
{
    options.AddOperationalStore(efOptions =>
    {
        // Use built-in TickerQDbContext with connection string
        efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
        {
            // dotnet ef migrations add TickerQInitialCreate --context TickerQDbContext --project ./src/ReData.DemoApplication -o ./Jobs/Migrations
            optionsBuilder.UseNpgsql(tickerQConnectinoString,
                builder =>
                {
                    builder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), ["40P01"]);
                    builder.MigrationsAssembly("ReData.DemoApplication");
                });
        }, schema: "tickerq");
    });
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/api/tickerq");
    });
});

services.AddDbContext<ApplicationDatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

services.AddSingleton<ConnectionService>();

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

app.Migrate<ApplicationDatabaseContext>();
app.Migrate<TickerQDbContext>();


app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseFastEndpoints();

app.UseSwaggerGen(options => { options.Path = "/openapi/{documentName}.json"; });
app.UseTickerQ();
app.MapScalarApiReference("api/docs");

app.Run();