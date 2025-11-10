using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Converters;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Extensions;
using ReData.DemoApplication.Repositories;
using ReData.DemoApplication.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

services.AddOutputCache();
services.AddFastEndpoints();
services.SwaggerDocument();

services.AddTransient<IRepository<DataSet>, DataSetRepository>();
services.AddDbContext<ApplicationDatabaseContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new ValueConverter());
    c.Serializer.Options.Converters.Add(new DataTypeJsonConverter());
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

app.UseSwaggerGen(options =>
{
    options.Path = "/openapi/{documentName}.json";
});
app.MapScalarApiReference();

app.Run();