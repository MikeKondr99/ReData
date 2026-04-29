using System.Diagnostics;
using System.Net;
using FastEndpoints;
using ReData.DataIO.DataExporters;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;
using ReData.DemoApp.Services;
using ReData.Query;
using StrictId;
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
    public required IDatasetRepository Datasets { get; init; }
    
    public required IConnectionService ConnectionService { get; init; }

    public override void Configure()
    {
        Get("/{Id}/export");
        Group<DataSetsGroup>();
        Description(d => d
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            .Produces(StatusCodes.Status200OK, contentType: "application/json")
            .Produces(StatusCodes.Status200OK, contentType: "application/vnd.apache.arrow.file")
            .Produces<ExportDatasetErrorResponse>(StatusCodes.Status400BadRequest, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError));
        AllowAnonymous();
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ExportDataSetRequest req, CancellationToken ct)
    {
        try
        {
            var dataset = await Datasets.GetByIdAsync(new Id<DatasetEntity>(req.Id), ct);

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

            if (applyResult.UnwrapErr(out var transformationError, out var qb))
            {
                await Send.ResponseAsync(
                    new ExportDatasetErrorResponse
                    {
                        Message = "Набор не может быть экспортирован: содержит невалидные трансформации.",
                        Index = transformationError.Index,
                        Errors = transformationError.Errors,
                    },
                    (int)HttpStatusCode.BadRequest,
                    ct);
                return;
            }

            var query = qb.Build();
            var runner = Factory.CreateQueryExecuter(DatabaseType.PostgreSql);
            await using var connection = await ConnectionService.GetConnectionAsync(ConnectionSource.DwhRead, ct);

            await using var reader = await runner.GetDataReaderAsync(query, connection);
            HttpContext.MarkResponseStart();
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = GetContentType(req.FileType);
            HttpContext.Response.Headers.Append("Content-Disposition",
                $"attachment; filename=\"{Uri.EscapeDataString(dataset.Name)}{GetExtension(req.FileType)}\";");
            var bodyStream = HttpContext.Response.Body;
            var exporter = GetExporter(req.FileType);
            await exporter.ExportAsync(reader, bodyStream, ct);
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
        ExportFileType.Json => "application/json",
        ExportFileType.Arrow => "application/vnd.apache.arrow.file",
        _ => throw new UnreachableException(),
    };

    private static string GetExtension(ExportFileType type) => type switch
    {
        ExportFileType.Csv => ".csv",
        ExportFileType.Excel => ".xlsx",
        ExportFileType.Json => ".json",
        ExportFileType.Arrow => ".arrow",
        _ => throw new UnreachableException(),
    };

    private static IDataExporter GetExporter(ExportFileType type) => type switch
    {
        ExportFileType.Csv => new SylvanCsvExporter(),
        ExportFileType.Excel => new SylvanExcelExporter(),
        ExportFileType.Json => new JsonDataExporter(),
        ExportFileType.Arrow => new ArrowFileDataExporter(),
        _ => throw new UnreachableException(),
    };
}
