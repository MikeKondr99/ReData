using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication;
using ReData.DemoApplication.Converters;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Extensions;
using ReData.DemoApplication.Requests;
using ReData.DemoApplication.Responses;
using ReData.DemoApplication.Services;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Impl.Functions;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

services.AddDbContext<ApplicationDatabaseContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new ValueConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;

});

services.AddSingleton<ConnectionService>();

var app = builder.Build();

// app.Migrate<ApplicationDatabaseContext>();
app.UseDefaultFiles();
app.UseStaticFiles();


app.MapControllers();

app.Run();