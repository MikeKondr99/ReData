using FastEndpoints;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Services;
using ReData.Query.Core.Types;
using Field = ReData.DemoApp.Database.Entities.Field;

namespace ReData.DemoApp.Commands;

public class CreateDataConnectorCommand : ICommand<DataConnectorEntity>
{
    public required string Name { get; init; }

    public required char Separator { get; init; }

    public required bool WithHeader { get; init; }

    public required Stream FileStream { get; init; }
}

public class CreateDataConnectorHandler : ICommandHandler<CreateDataConnectorCommand, DataConnectorEntity>
{
    public CreateDataConnectorHandler(
        ApplicationDatabaseContext db,
        DwhService dwh)
    {
        Db = db;
        DwhService = dwh;
    }
    public required ApplicationDatabaseContext Db { get; init; }
    public required DwhService DwhService { get; init; }

    /// <inheritdoc />
    public async Task<DataConnectorEntity> ExecuteAsync(CreateDataConnectorCommand command, CancellationToken ct)
    {
        var csvConfiguration = new CsvConfiguration()
        {
            Seperator = command.Separator, // req.Separator,
            ReadEmptyStringAsNull = true,
        };
        var query = command.FileStream.QueryAsync(
            excelType: ExcelType.CSV,
            useHeaderRow: false, // false for now
            configuration: csvConfiguration,
            cancellationToken: ct
        );

        string[]? columns = null;
        var tableId = Guid.NewGuid();
        string tableName = $"table_{tableId}";

        using (var connection = new NpgsqlConnection(DwhService.WriteConnection))
        {
            await connection.OpenAsync(ct);
            await using (var transaction = await connection.BeginTransactionAsync(ct))
            {
                columns = await CopyRowsAsync(
                    connection: connection,
                    transaction: transaction,
                    tableName: tableName,
                    withHeader: command.WithHeader,
                    rows: query,
                    ct: ct
                );
                await transaction.CommitAsync(ct);
            }
        }

        var entity = new DataConnectorEntity()
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            TableName = tableName,
            FieldList = columns.Select(c => new Field()
            {
                Alias = c,
                DataType = DataType.Text,
                CanBeNull = true,
            }).ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        Db.DataConnectors.Add(entity);
        await Db.SaveChangesAsync(ct);

        return entity;
    }

    private static async Task<string> CreateTableAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        string[] columns)
    {
        var createTableSql = $"""
                              CREATE TABLE "{tableName}" (
                                  rownum SERIAL PRIMARY KEY,
                                  {columns.Select(c => $"\"{c}\" TEXT").JoinBy(",\n")}
                              )
                              """;

        using var command = new NpgsqlCommand(createTableSql, connection, transaction);
        await command.ExecuteNonQueryAsync();
        return tableName;
    }

    private static async Task<string[]> CopyRowsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        bool withHeader,
        IAsyncEnumerable<dynamic> rows,
        CancellationToken ct
    )
    {
        var iter = rows.GetAsyncEnumerator(ct);
        if (!await iter.MoveNextAsync())
        {
            // 0 записей
            throw new Exception();
        }

        string[] columnKeys = (iter.Current as IDictionary<string, object>)!.Keys.ToArray();
        string[]? columns = null;
        if (withHeader)
        {
            columns = (iter.Current as IDictionary<string, object>)!.Values.Select(v => v.ToString()!).ToArray();
        }
        else
        {
            columns = columnKeys;
        }

        // var columnNames = Enumerable.Range(1, columns.Length).Select(col => $"column{col}").ToArray();

        await CreateTableAsync(connection, transaction, tableName, columns);

        using (var writer =
               connection.BeginBinaryImport(
                   $"COPY \"{tableName}\" ({columns.Select(c => $"\"{c}\"").JoinBy(", ")}) FROM STDIN (FORMAT BINARY)"))
        {
            // Если без хедера то обрабатывает первую запись как запись
            if (!withHeader)
            {
                var row = iter.Current as IDictionary<string, object?>;
                await writer.StartRowAsync(ct);
                for (var i = 0; i < columns.Length; i++)
                {
                    var value = row[columnKeys[i]]?.ToString();
                    await writer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Text, ct);
                }
            }

            while (await iter.MoveNextAsync())
            {
                var row = iter.Current as IDictionary<string, object?>;
                await writer.StartRowAsync(ct);

                for (var i = 0; i < columns.Length; i++)
                {
                    var value = row[columnKeys[i]]?.ToString();
                    await writer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Text, ct);
                }
            }

            await writer.CompleteAsync(ct);
            return columns;
        }
    }
}