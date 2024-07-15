using System.Text.Json.Serialization;
using FluentValidation;
using ReData.Application;
using ReData.Database;
using ReData.Domain;
using ReData.Domain.Repositories;
using ReData.Domain.Validators;
using SimpleInjector;

var builder = WebApplication.CreateBuilder(args);


var services = builder.Services;

services.AddControllers()
    .AddControllersAsServices()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
services.AddLocalization(options => options.ResourcesPath = "Resources");
services.AddLogging();

var container = new SimpleInjector.Container();
container.Options.PropertySelectionBehavior = new InjectPropertySelectionBehavior();
services.AddSimpleInjector(container, options =>
{
    options.AddAspNetCore()
        .AddControllerActivation();

    options.AddLogging();
    options.AddLocalization();
});

services.AddAutoMapper(typeof(Program), typeof(IRepository<>));
services.AddDbContext<ApplicationDatabaseContext>();

container.Register<IValidator<ReData.Domain.DataSource>,DataSourceValidator>(Lifestyle.Scoped);

container.Register<IDatabase, ApplicationDatabaseContext>(Lifestyle.Scoped);
container.Register<IRepository<DataSource>,DataSourceRepository>(Lifestyle.Scoped);

container.RegisterDecorator<IRepository<DataSource>,ValidatedRepository<ReData.Domain.DataSource>>(Lifestyle.Scoped);

var app = builder.Build();

app.Services.UseSimpleInjector(container);

app.Migrate<ApplicationDatabaseContext>();

app.MapControllers();

app.Run();
