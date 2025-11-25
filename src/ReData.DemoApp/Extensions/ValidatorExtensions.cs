using ReData.DemoApp.Database;

namespace ReData.DemoApp.Extensions;

public static class ValidatorExtensions
{
    public static ApplicationDatabaseContext Db<T>(this FastEndpoints.Validator<T> validator) =>
        validator.Resolve<ApplicationDatabaseContext>();
}