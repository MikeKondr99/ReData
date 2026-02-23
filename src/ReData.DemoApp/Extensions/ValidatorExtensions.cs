using System.Diagnostics;
using ReData.DemoApp.Database;

namespace ReData.DemoApp.Extensions;

public static class ValidatorExtensions
{
    public static ApplicationDatabaseContext Db<T>(this FastEndpoints.Validator<T> validator)
        where T : notnull =>
        validator.Resolve<ApplicationDatabaseContext>();
}

public static class Tracing
{
    public static ActivitySource ReData { get; } = new("ReData.App");
}