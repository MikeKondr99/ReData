using FastEndpoints;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Services;
using ReData.Query.Core.Types;

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
        if (command.FileStream is null)
        {
            throw new ArgumentNullException("command.FileStream", "Must be not null");
        }
        var csvConfiguration = new CsvConfiguration()
        {
            Seperator = command.Separator,
            ReadEmptyStringAsNull = true,
        };
        var query = command.FileStream.QueryAsync(
            excelType: ExcelType.CSV,
            useHeaderRow: false, // false for now
            configuration: csvConfiguration,
            cancellationToken: ct
        );

        string[]? aliases = null;
        var tableId = Guid.NewGuid();
        string tableName = $"table_{tableId}";

        using (var connection = new NpgsqlConnection(DwhService.WriteConnection))
        {
            await connection.OpenAsync(ct);
            await using (var transaction = await connection.BeginTransactionAsync(ct))
            {
                aliases = await CopyRowsAsync(
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
            FieldList = aliases.Select((c, i) => new DataConnectorField()
            {
                Alias = c,
                Column = $"column{i + 1}",
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
        IEnumerable<string> columns)
    {
        var createTableSql = $"""
                              CREATE TABLE "{tableName}" (
                                  rownum SERIAL PRIMARY KEY,
                                  {columns.Select(c => $"{c} TEXT").JoinBy(",\n")}
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

        string[] fieldKeys = (iter.Current as IDictionary<string, object>)!.Keys.ToArray();
        string[]? aliases = null;
        if (withHeader)
        {
            aliases = (iter.Current as IDictionary<string, object>)!.Values.Select(v => v.ToString()!).ToArray();
            for (int i = 0; i < aliases.Length; i++)
            {
                var counter = 1;
                var alias = aliases[i];
                while (aliases[..i].Contains(aliases[i]))
                {
                    counter += 1;
                    aliases[i] = $"{alias}_{counter}";
                }
            }
        }
        else
        {
            aliases = fieldKeys;
        }

        var columns = Enumerable.Range(1, aliases.Length).Select(col => $"\"column{col}\"");

        await CreateTableAsync(connection, transaction, tableName, columns);

        using (var writer =
               connection.BeginBinaryImport(
                   $"COPY \"{tableName}\" ({columns.JoinBy(", ")}) FROM STDIN (FORMAT BINARY)"))
        {
            // Если без хедера то обрабатывает первую запись как запись
            if (!withHeader)
            {
                var row = iter.Current as IDictionary<string, object?>;
                await writer.StartRowAsync(ct);
                for (var i = 0; i < aliases.Length; i++)
                {
                    var value = row[fieldKeys[i]]?.ToString();
                    await writer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Text, ct);
                }
            }

            while (await iter.MoveNextAsync())
            {
                var row = iter.Current as IDictionary<string, object?>;
                await writer.StartRowAsync(ct);

                for (var i = 0; i < aliases.Length; i++)
                {
                    var value = row[fieldKeys[i]]?.ToString();
                    await writer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Text, ct);
                }
            }

            await writer.CompleteAsync(ct);
            return aliases;
        }
    }
}