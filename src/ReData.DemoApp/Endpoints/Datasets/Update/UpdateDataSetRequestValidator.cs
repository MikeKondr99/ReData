using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Repositories.Datasets;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

public sealed class UpdateDataSetRequestValidator : Validator<UpdateDataSetRequest> 
{
    public UpdateDataSetRequestValidator()
    {
        RuleFor(req => req.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .MinimumLength(3)
            .MustAsync(async (req, name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return true;
                }

                var existing = await this.Resolve<IDatasetRepository>().GetByNameAsync(name, ct);
                return existing is null || existing.Id == req.Id;
            });
        
        RuleFor(req => req.Transformations)
            .NotNull();

        RuleFor(req => req.ConnectorId)
            .NotNull()
            .MustAsync((connectorId, ct) => this.Db().DataConnectors.AnyAsync(dc => dc.Id == connectorId, ct));
    }
    
}
