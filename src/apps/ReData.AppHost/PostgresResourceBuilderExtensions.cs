using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace Aspire.Hosting;

public static class PostgresResourceBuilderExtensions
{
    private const string PostgresAdminToolConfigKey = "Development:PostgresAdminTool";

    public static IResourceBuilder<PostgresServerResource> WithPgTool(
        this IResourceBuilder<PostgresServerResource> builder,
        IConfiguration configuration)
    {
        var tool = configuration[PostgresAdminToolConfigKey];
        Console.WriteLine(tool);

        if (string.IsNullOrWhiteSpace(tool))
        {
            return builder;
        }

        if (string.Equals(tool, "PgAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return builder.WithPgAdmin();
        }

        if (string.Equals(tool, "PgWeb", StringComparison.OrdinalIgnoreCase))
        {
            return builder.WithPgWeb();
        }

        throw new InvalidOperationException(
            $"Unsupported {PostgresAdminToolConfigKey} value '{tool}'. Supported values: null, PgAdmin, PgWeb.");
    }
}
