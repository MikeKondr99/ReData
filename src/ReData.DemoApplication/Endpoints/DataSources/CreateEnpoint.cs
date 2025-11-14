using System.Dynamic;
using ClickHouse.Client.Utility;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Services;
using ReData.Query.Core.Types;
using Field = ReData.DemoApplication.Database.Entities.Field;

namespace ReData.DemoApplication.Endpoints.DataSources;

public class CreateDataSourceEndpoint : Endpoint<CreateDataSourceRequest,
    Results<Created<CreateDataSourceResponse>, BadRequest<string>>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required ConnectionService ConnectionService { get; init; }

    public override void Configure()
    {
        Post("/api/datasources");
        AllowAnonymous();
        AllowFileUploads(dontAutoBindFormData: true);
        // MaxRequestBodySize(50 * 1024 * 1024);
    }

    public override async Task<Results<Created<CreateDataSourceResponse>, BadRequest<string>>> ExecuteAsync(
        CreateDataSourceRequest req, CancellationToken ct)
    {
        throw new NotImplementedException();
        req.Name = Query<string>("name");
        req.Separator = Query<char>("separator");
        req.WithHeader = Query<bool>("withHeader");
        Stream? fileStream = (await FormMultipartSectionsAsync(ct).FirstOrDefaultAsync()).FileSection.Section.Body;

        // using MiniExcel for properly reading csv
        var csvConfiguration = new CsvConfiguration()
        {
            Seperator = req.Separator, // always ',' for now
            ReadEmptyStringAsNull = true,
        };
        var query = fileStream.QueryAsync(
            excelType: ExcelType.CSV,
            useHeaderRow: false, // false for now
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
                    withHeader: req.WithHeader,
                    rows: query,
                    ct: ct
                );
                await transaction.CommitAsync(ct);
            }
        }


        // Db.DataSources.Add(entity);
        // await Db.SaveChangesAsync(ct);



        // return TypedResults.Created("lol", response);
    }

    private static async Task<string> CreateTableAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        int columnCount)
    {
        var columnDefinitions = string.Join(", ", Enumerable.Range(1, columnCount).Select(col => $"column{col} TEXT"));

        var createTableSql = $"""
                              CREATE TABLE "{tableName}" (
                                  id SERIAL PRIMARY KEY,
                                  {columnDefinitions}
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

        var columnNames = Enumerable.Range(1, columns.Length).Select(col => $"column{col}").ToArray();

        await CreateTableAsync(connection, transaction, tableName, columns.Length);

        using (var writer =
               connection.BeginBinaryImport(
                   $"COPY \"{tableName}\" ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)"))
        {
            // Если без хедера то обрабатывает первую запись как запись
            if (!withHeader)
            {
                var row = iter.Current as IDictionary<string,object?>;
                await writer.StartRowAsync(ct);
                for (var i = 0; i < columns.Length; i++)
                {
                    var value = row[columnKeys[i]]?.ToString();
                    await writer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Text, ct);
                }
            }

            while (await iter.MoveNextAsync())
            {
                var row = iter.Current as IDictionary<string,object?>;
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