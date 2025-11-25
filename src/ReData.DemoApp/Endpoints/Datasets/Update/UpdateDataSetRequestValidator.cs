using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Extensions;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

public sealed class UpdateDataSetRequestValidator : Validator<UpdateDataSetRequest> 
{
    public UpdateDataSetRequestValidator(ApplicationDatabaseContext db)
    {
        RuleFor(req => req.Name)
            .NotNull()
            .MinimumLength(3)
            .MustAsync((req,name, ct) => this.Db().DataSets.AllAsync(ds => ds.Name != name || ds.Id == req.Id, ct));
        
        RuleFor(req => req.Transformations)
            .NotNull();

        RuleFor(req => req.ConnectorId)
            .NotNull()
            .MustAsync((connectorId, ct) => this.Db().DataConnectors.AnyAsync(dc => dc.Id == connectorId, ct));
    }
    
}
