using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var angularEnabled = builder.Configuration.GetValue("Development:Frontends:Angular:Enabled", true);
var svelteEnabled = builder.Configuration.GetValue("Development:Frontends:Svelte:Enabled", true);

#pragma warning disable ASPIREPOSTGRES001

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithDataVolume()
    .WithOtlpExporter()
    .WithRealmImport("./realms");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgTool(builder.Configuration);

var mainDatabase = postgres.AddDatabase("redata").WithPostgresMcp();
var tickerqDatabase = postgres.AddDatabase("tickerq").WithPostgresMcp();
var dwh = postgres.AddDatabase("dwh").WithPostgresMcp();



var api = builder.AddProject<Projects.ReData_DemoApp>("redata-demoapp")
    .WithReference(keycloak)
    .WithReference(mainDatabase).WaitFor(mainDatabase)
    .WithReference(tickerqDatabase).WaitFor(tickerqDatabase)
    .WithReference(dwh);

if (angularEnabled)
{
    builder.AddNpmApp("redata-angular", "../ReData.Angular", "start")
        .WithHttpEndpoint(port: 64200, targetPort: 4200)
        .WithEnvironment("KEYCLOAK_HTTP", keycloak.GetEndpoint("http"))
        .WithReference(keycloak).WaitFor(keycloak)
        .WithReference(api).WaitFor(api)
        .WithHttpHealthCheck("/");
}

if (svelteEnabled)
{
    builder.AddNpmApp("redata-svelte", "../ReData.Svelte", "dev")
        .WithHttpEndpoint(port: 64201, targetPort: 5173)
        .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
        .WithReference(keycloak).WaitFor(keycloak)
        .WithReference(api).WaitFor(api)
        .WithHttpHealthCheck("/");
}

builder.Build().Run();
