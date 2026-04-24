using FastEndpoints;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Transformations;
using ReData.Query.Executors;

namespace ReData.DemoApp.Commands;

public record Metadata
{
    public required IReadOnlyList<DataSetField>? FieldList { get; init; }

    public required int? RowsCount { get; init; }
}

public sealed class GetMetadataCommand : ICommand<Metadata>
{
    public required Guid ConnectorId { get; init; }

    public required IReadOnlyList<Transformation> Transformations { get; init; }
}

public sealed class GetMetadataCommandHandler() : ICommandHandler<GetMetadataCommand, Metadata>
{
    /// <inheritdoc />
    public async Task<Metadata> ExecuteAsync(GetMetadataCommand command, CancellationToken ct)
    {
        IReadOnlyList<DataSetField>? fieldList = null;
        int? rowsCount = null;
        var queryBuilder = (await new ApplyTransformationsCommand()
        {
            Transformations = command.Transformations.ToArray(),
            DataConnectorId = command.ConnectorId
        }.ExecuteAsync(ct)).UnwrapOrDefault();
        if (queryBuilder is not null)
        {
            var query = queryBuilder.Build();
            fieldList = query.Fields().Select(f => new DataSetField()
            {
                Alias = f.Alias,
                DataType = f.Type.Type,
                CanBeNull = f.Type.CanBeNull,
            }).ToArray();

            var execution = (await new ExecuteQueryCommand()
            {
                Query = queryBuilder.Select(new()
                {
                    ["COUNT"] = "COUNT()"
                }).Unwrap().Build(),
            }.ExecuteAsync(ct)).UnwrapOrDefault();

            if (execution is not null)
            {
                await using (execution.DataReader)
                await using (execution.Connection)
                {
                    var count = await execution.DataReader.GetScalarAsync<long>(ct);
                    rowsCount = checked((int)count);
                }
            }
        }

        return new Metadata()
        {
            FieldList = fieldList,
            RowsCount = rowsCount,
        };
    }
}
