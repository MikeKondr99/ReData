using System.Text.Json.Serialization;
using ReData.Application;
using ReData.Database;
using SimpleInjector;

var builder = WebApplication.CreateBuilder(args);


var container = builder.Services.RegisterServices();

var app = builder.Build();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();


app.Services.UseSimpleInjector(container);

app.Migrate<ApplicationDatabaseContext>();

app.MapFallbackToFile("/index.html");

await app.RunAsync();