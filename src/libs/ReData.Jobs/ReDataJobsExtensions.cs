using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Instrumentation.OpenTelemetry;

namespace ReData.Jobs;

public static class ReDataJobsExtensions
{
    private const string TestJobFunctionName = "ReData test job";
    private const string DefaultDashboardBasePath = "/api/tickerq";
    private const string DefaultDashboardLogin = "test";
    private const string DefaultDashboardPassword = "secret9";
    private const string ProducerNodeIdentifier = "producer-dashboard";
    private const string WorkerFallbackNodeIdentifier = "job-worker";

    public static WebApplicationBuilder AddReDataJobs(
        this WebApplicationBuilder builder,
        ReDataJobsMode mode)
    {
        builder.Services.AddTickerQ(options =>
        {
            options.AddOpenTelemetryInstrumentation();
            ConfigureScheduler(options, builder, mode);
            ConfigureOperationalStore(options, builder, mode);

            if (mode == ReDataJobsMode.ProducerDashboard)
            {
                options.DisableBackgroundServices();
                options.AddDashboard(dashboardOptions =>
                {
                    dashboardOptions.SetBasePath(DefaultDashboardBasePath);
                    dashboardOptions.WithBasicAuth(DefaultDashboardLogin, DefaultDashboardPassword);
                });
            }
        });

        MapTickerFunctions(builder.Services);
        return builder;
    }

    public static string GetTestJobFunctionName()
    {
        return TestJobFunctionName;
    }

    private static void ConfigureScheduler(
        TickerOptionsBuilder<TimeTickerEntity, CronTickerEntity> options,
        WebApplicationBuilder builder,
        ReDataJobsMode mode)
    {
        options.ConfigureScheduler(scheduler =>
        {
            scheduler.MaxConcurrency = mode == ReDataJobsMode.Worker ? 4 : 8;
            scheduler.NodeIdentifier = mode == ReDataJobsMode.Worker
                ? builder.Configuration["TickerQ:NodeIdentifier"] ?? WorkerFallbackNodeIdentifier
                : ProducerNodeIdentifier;
        });
    }

    private static void ConfigureOperationalStore(
        TickerOptionsBuilder<TimeTickerEntity, CronTickerEntity> options,
        WebApplicationBuilder builder,
        ReDataJobsMode mode)
    {
        options.AddOperationalStore(efOptions =>
        {
            efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
            {
                optionsBuilder.UseNpgsql(
                    builder.Configuration.GetConnectionString("TickerQ"),
                    npgsqlOptions =>
                    {
                        if (mode == ReDataJobsMode.ProducerDashboard)
                        {
                            npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), ["40P01"]);
                            npgsqlOptions.MigrationsAssembly("ReData.DemoApp");
                        }
                    });

                optionsBuilder.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                });
            });
        });
    }

    private static void MapTickerFunctions(IServiceCollection services)
    {
        services.MapTickerGroup(string.Empty)
            .MapTicker<TestJob>(TestJobFunctionName);
    }
}
