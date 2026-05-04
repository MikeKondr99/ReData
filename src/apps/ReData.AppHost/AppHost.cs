var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPOSTGRES001

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithDataVolume()
    .WithRealmImport("./realms");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var mainDatabase = postgres.AddDatabase("redata").WithPostgresMcp();
var tickerqDatabase = postgres.AddDatabase("tickerq").WithPostgresMcp();
var dwh = postgres.AddDatabase("dwh").WithPostgresMcp();



var api = builder.AddProject<Projects.ReData_DemoApp>("redata-demoapp")
    .WithReference(keycloak)
    .WithReference(mainDatabase).WaitFor(mainDatabase)
    .WithReference(tickerqDatabase).WaitFor(tickerqDatabase)
    .WithReference(dwh);

builder.AddNpmApp("redata-angular", "../ReData.Angular", "start")
    .WithHttpEndpoint(port: 64200, targetPort: 4200)
    .WithEnvironment("KEYCLOAK_HTTP", keycloak.GetEndpoint("http"))
    .WithReference(keycloak)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
