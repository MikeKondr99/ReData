using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Endpoints.Datasets;
using ReData.DemoApplication.Services;
using ReData.Query.Core.Types;
using Field = ReData.DemoApplication.Database.Entities.Field;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class
    UploadFileEndpoint : Endpoint<UploadFileRequest, Results<Ok<UploadFileResponse>, BadRequest<string>>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required ConnectionService ConnectionService { get; init; }

    public override void Configure()
    {
        Post("/api/datasets/upload");
        AllowAnonymous();
        AllowFileUploads(dontAutoBindFormData: true);
    }

    public override async Task<Results<Ok<UploadFileResponse>, BadRequest<string>>> ExecuteAsync(
        UploadFileRequest req,
        CancellationToken ct
    )
    {
        Stream? fileStream = (await FormMultipartSectionsAsync(ct).FirstOrDefaultAsync()).FileSection.Section.Body;

        // using MiniExcel for properly reading csv
        var csvConfiguration = new CsvConfiguration()
        {
            Seperator = ',', // always ',' for now
            ReadEmptyStringAsNull = true,
        };

        var query = fileStream.QueryAsync(
            excelType: ExcelType.CSV,
            useHeaderRow: false,
            configuration: csvConfiguration,
            cancellationToken: ct
        );

        string[]? columns = null;
        var tableId = Guid.NewGuid();
        string tableName = $"table_{tableId}";

        using (var connection = new NpgsqlConnection(ConnectionService.Connection))
        {
            await connection.OpenAsync(ct);
            await using (var transaction = await connection.BeginTransactionAsync(ct))
            {
                columns = await CopyRowsAsync(
                    connection: connection,
                    transaction: transaction,
                    tableName: tableName,
                    rows: query,
                    ct: ct
                );
                await transaction.CommitAsync(ct);
            }
        }

        List<Field> fields =
        [
            new Field()
            {
                Alias = "rownum",
                DataType = DataType.Integer,
                CanBeNull = false,
            }
        ];

        fields.AddRange(columns.Select(c => new Field()
        {
            Alias = c,
            DataType = DataType.Text,
            CanBeNull = true,
        }));

        var response = new UploadFileResponse()
        {
            TableId = tableId,
            FieldList = fields,
        };

        return TypedResults.Ok(response);
    }

    private static async Task<string> CreateTableAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        string[] columns)
    {
        var createTableSql = $"""
                              CREATE UNLOGGED TABLE "{tableName}" (
                                  "rownum" SERIAL PRIMARY KEY, {columns.Select(c => $"\"{c}\" TEXT").JoinBy(", ")}
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

        columns = (iter.Current as IDictionary<string, object>)!.Values.Select(v => v.ToString()!).ToArray();

        await CreateTableAsync(connection, transaction, tableName, columns);

        using (var writer =
               connection.BeginBinaryImport(
                   $"COPY \"{tableName}\" ({columns.Select(c => $"\"{c}\"").JoinBy(", ")}) FROM STDIN (FORMAT BINARY)"))
        {
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