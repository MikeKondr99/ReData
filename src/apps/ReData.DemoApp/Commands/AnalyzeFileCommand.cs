using System.Data.Common;
using FastEndpoints;
using ReData.DataIO.DataImporters;

namespace ReData.DemoApp.Commands;

public sealed class AnalyzeFileCommand : ICommand<DbDataReader>
{
    public required char Separator { get; init; }
    
    public required bool HasHeaders { get; init; }
    public required Stream FileStream { get; init; }
}

public sealed class AnalyzeFileCommandHandler : ICommandHandler<AnalyzeFileCommand, DbDataReader>
{
    public async Task<DbDataReader> ExecuteAsync(AnalyzeFileCommand command, CancellationToken ct)
    {
        var rawReader = await new SylvanCsvDataImporter(command.Separator, command.HasHeaders).ImportAsync(command.FileStream, ct);
        var reader = await new DataAnalyzer().AnalyzeAsync(rawReader, ct);
        return reader;
    }
}