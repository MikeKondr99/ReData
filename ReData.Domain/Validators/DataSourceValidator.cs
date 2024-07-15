using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ReData.Database;
using ReData.Database.Entities;

namespace ReData.Domain.Validators;

public class DataSourceValidator : AbstractValidator<DataSource>
{
    public required IDatabase Database { private get; init; }
    public DataSourceValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .NotEqual(DataSourceType.Unknown);

        RuleFor(x => x.Name)
            .MustAsync(async (ds, name, ct) =>
            {
                var result = await Database!
                    .Set<Entity.DataSource>()
                    .Where(x => x.Name == name && x.Id != ds.Id)
                    .AnyAsync();
                return !result;
            })
            .WithMessage("Must be unique");
        

        When(x => x.Type == DataSourceType.PostgreSql, () =>
        {
            RuleFor(x => x.Parameters).RequiredParameters(["Host"]);
        });

        When(x => x.Type == DataSourceType.Csv, () =>
        {
            RuleFor(x => x.Parameters).Must(p => p.ContainsKey("path"));
        });
    }
    
    
}