using System.Text.Json.Serialization;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ReData.Application.Controllers.DataSource;
using ReData.Database;
using ReData.Domain;
using ReData.Domain.Repositories;
using ReData.Domain.Services;
using ReData.Domain.Services.QueryBuilder;
using ReData.Domain.Validators;
using SimpleInjector;

namespace ReData.Application;

public static class Services
{
    public static Container RegisterServices(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        services.AddLogging();

        services.AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        var container = new Container();
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
        
        container.Register<IValidator<DataSource>,DataSourceValidator>(Lifestyle.Scoped);

        container.Register<IDatabase, ApplicationDatabaseContext>(Lifestyle.Scoped);
        
        container.Register<IRepository<DataSource>,DataSourceRepository>(Lifestyle.Scoped);
        container.RegisterDecorator<IRepository<DataSource>, ValidatedRepository<DataSource>>(Lifestyle.Scoped);

        container.Collection.Register<IDataSourceConnector>([typeof(PostgresConnector), typeof(CsvConnector)]);
        
        container.Register<IDataSourceConnectorFactory, DataSourceConnectionCheckFactory>(Lifestyle.Singleton);
        
        container.Register<DataSourceService>();
        
        return container;
        
    }
    
}

