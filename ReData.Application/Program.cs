using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Progress;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using ReData.Application;
using ReData.Database;
using SimpleInjector;

var builder = WebApplication.CreateBuilder(args);


var container = builder.Services.RegisterServices();


var app = builder.Build();

app.MapControllers();


app.UseDefaultFiles();

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "\\ReDataClient";

    spa.UseAngularCliServer(npmScript: "start");
});
app.UseStaticFiles();

app.UseHttpsRedirection();

app.Services.UseSimpleInjector(container);

app.Migrate<ApplicationDatabaseContext>();

app.MapFallbackToFile("/index.html");

await app.RunAsync();

