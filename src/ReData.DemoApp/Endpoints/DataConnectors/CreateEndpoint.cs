using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.DataSets;
using ReData.DemoApp.Services;
using ReData.Query.Core.Types;
using Entities_Field = ReData.DemoApp.Database.Entities.Field;
using Field = ReData.DemoApp.Database.Entities.Field;

namespace ReData.DemoApp.Endpoints.DataSources;

public class CreateDataSourceEndpoint : Endpoint<CreateDataConnectorRequest,
    Results<Created<CreateDataConnectorResponse>, BadRequest<string>>>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required DwhService DwhService { get; init; }
    
    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Post("/api/data-connectors");
        AllowAnonymous();
        AllowFileUploads(dontAutoBindFormData: true);
        // MaxRequestBodySize(50 * 1024 * 1024);
    }

    public override async Task<Results<Created<CreateDataConnectorResponse>, BadRequest<string>>> ExecuteAsync(
        CreateDataConnectorRequest req, CancellationToken ct)
    {
        req.Name = Query<string>("name")!;
        // req.Separator = Query<char>("separator");
        // req.WithHeader = Query<bool>("withHeader");
        Stream? fileStream = (await FormMultipartSectionsAsync(ct).FirstOrDefaultAsync()).FileSection?.Section.Body;

        var csvConfiguration = new CsvConfiguration()
        {
            Seperator = req.Separator, // req.Separator,
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

        using (var connection = new NpgsqlConnection(DwhService.DwhWriteConnection))
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

        var entity = new DataConnectorEntity()
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            TableName = tableName,
            FieldList = columns.Select(c => new Entities_Field
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


        var response = new CreateDataConnectorResponse()
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        await OutputCache.EvictByTagAsync("data-connectors", ct);
        
        return TypedResults.Created("lol", response);
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