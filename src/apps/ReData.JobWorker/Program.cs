using Microsoft.EntityFrameworkCore;
using ReData.Jobs;
using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddReDataJobs(ReDataJobsMode.Worker);

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/", (IConfiguration configuration) =>
{
    var apiUrl = configuration["services:redata-demoapp:http:0"]
        ?? configuration["REDATA_DEMOAPP_HTTP"];

    if (string.IsNullOrWhiteSpace(apiUrl))
    {
        return Results.Ok("I am a worker, dashboard not found.");
    }

    var dashboardUrl = $"{apiUrl.TrimEnd('/')}/api/tickerq";
    return Results.Redirect(dashboardUrl, permanent: false);
});
app.UseTickerQ();

await app.RunAsync();
