using ReData.Query.Common;

namespace ReData.DemoApp.Endpoints.Datasets.Export;

public sealed record ExportDatasetErrorResponse
{
    public required string Message { get; init; }

    public required int Index { get; init; }

    public required IEnumerable<IReadOnlyList<ExprError>>? Errors { get; init; }
}
