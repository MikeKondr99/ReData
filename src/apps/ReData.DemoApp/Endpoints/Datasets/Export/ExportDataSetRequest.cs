using Microsoft.AspNetCore.Mvc;

namespace ReData.DemoApp.Endpoints.Datasets.Export;

public sealed class ExportDataSetRequest
{
    [FromRoute]
    public required Guid Id { get; init; }

    [FromQuery]
    public ExportFileType FileType { get; init; } = ExportFileType.Csv;
}
