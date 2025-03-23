
var builder = DistributedApplication
    .CreateBuilder(args);

// var keycloak = builder
//     .AddKeycloakContainer("app-keycloak")
//     .WithDataVolume()
//     .WithImport("./KeycloakConfiguration/Test-realm.json")
//     .WithImport("./KeycloakConfiguration/Test-users-0.json")
//     .WithHttpHealthCheck();

// var realm = keycloak.AddRealm("Test");

var postgres = builder
    .AddPostgres("redata-postgres")
    .WithDataVolume()
    // .WithPgAdmin()
    ;

var db = postgres
    .AddDatabase("redata-postgres-db");

// var api = builder
//     .AddProject<Projects.ReData_Application>("redata-api")
//     .WithHttpEndpoint()
//     // .WithReference(realm)
//     .WithReference(db)
//     .WaitFor(db)
//     // .WaitFor(realm);
    ;

builder.AddNpmApp("app-angular", "../ReData.Angular")
    // .WithReference(api)
    //.WithReference(realm)
    // .WaitFor(api)
    .WithHttpEndpoint(env:"PORT")
    .WithHttpHealthCheck()
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();