using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Repositories.Datasets;

namespace ReData.DemoApp.Endpoints.Datasets.Create;

public sealed class CreateDataSetRequestValidator : Validator<CreateDataSetRequest> 
{
    public CreateDataSetRequestValidator()
    {
        RuleFor(req => req.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .MinimumLength(3)
            .MustAsync(async (name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return true;
                }

                return await Resolve<IDatasetRepository>().GetByNameAsync(name, ct) is null;
            });
        
        RuleFor(req => req.Transformations)
            .NotNull();
        
        RuleFor(req => req.ConnectorId)
            .NotNull()
            .MustAsync((connectorId, ct) => this.Db().DataConnectors.AnyAsync(dc => dc.Id == connectorId, ct));
    }
    
}
