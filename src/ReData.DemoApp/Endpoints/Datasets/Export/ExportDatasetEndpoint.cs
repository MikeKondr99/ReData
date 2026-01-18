using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Org.BouncyCastle.Ocsp;
using ReData.DataExporter;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Services;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Runners;
using Factory = ReData.Query.Factory;

namespace ReData.DemoApp.Endpoints.Datasets.Export;

/// <summary>
/// Экспортировать данные набора
/// </summary>
/// <remarks>
/// Возвращает данные набора файлом в указанном формате
/// </remarks>
public class ExportDatasetEndpoint : Endpoint<ExportDataSetRequest>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public required DwhService DwhService { get; init; }

    public override void Configure()
    {
        Get("/{Id}/export");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ExportDataSetRequest req, CancellationToken ct)
    {
        try
        {
            var dataset = await Db.DataSets
                .Include(ds => ds.Transformations)
                .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

            if (dataset is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var applyResult = await new ApplyTransformationsCommand()
            {
                DataConnectorId = dataset.DataConnectorId,
                Transformations = dataset.Transformations
                    .Where(tb => tb.Enabled)
                    .OrderBy(tb => tb.Order)
                    .Select(tb => tb.Data)
                    .ToArray(),
            }.ExecuteAsync(ct);

            QueryBuilder qb = applyResult.Expect("Нет проверки если трансформации с ошибкой");
            var runner = Factory.CreateQueryRunner(DatabaseType.PostgreSql);
            await using var connection = new NpgsqlConnection(DwhService.ReadConnection);

            var rawReader = await runner.GetDataReaderAsync(qb.Build(), connection);
            await using var reader = rawReader = new MyDbDataReader(rawReader, qb.Build().Fields());
            HttpContext.MarkResponseStart();
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = GetContentType(req.FileType);
            // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
            HttpContext.Response.Headers.Append("Content-Disposition",
                $"attachment; filename=\"{Uri.EscapeDataString(dataset.Name)}{GetExtension(req.FileType)}\";");
            var bodyStream = HttpContext.Response.Body;
            var exporter = GetExporter(req.FileType);
            await exporter.ExportAsync(rawReader, bodyStream, ct);
        }
        catch (Exception)
        {
            await Send.StatusCodeAsync(500, ct);
        }
    }

    private static string GetContentType(ExportFileType type) => type switch
    {
        ExportFileType.Csv => "text/csv",
        ExportFileType.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ExportFileType.Json => "application/json"
    };

    private static string GetExtension(ExportFileType type) => type switch
    {
        ExportFileType.Csv => ".csv",
        ExportFileType.Excel => ".xlsx",
        ExportFileType.Json => ".json"
    };

    private static IDataExporter GetExporter(ExportFileType type) => type switch
    {
        ExportFileType.Csv => new SylvanCsvExporter(),
        ExportFileType.Excel => new SylvanExcelExporter(),
        ExportFileType.Json => new JsonDataExporter(),
    };



}