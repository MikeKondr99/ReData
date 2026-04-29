using System.Data.Common;
using FastEndpoints;
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


public class CreateDataConnectorHandler(IConnectionService connectionService, ApplicationDatabaseContext db) : ICommandHandler<CreateDataConnectorCommand, DataConnectorEntity>
{
    public async Task<DataConnectorEntity> ExecuteAsync(CreateDataConnectorCommand command, CancellationToken ct)
    {
        await using var reader = await new AnalyzeFileCommand()
        {
            FileStream = command.FileStream,
            Separator = command.Separator,
            HasHeaders = command.WithHeader,
        }.ExecuteAsync(ct);

        var aliases = Enumerable
            .Range(0, reader.FieldCount)
            .Select(i => reader.GetName(i))
            .ToArray();

        var types = Enumerable
            .Range(0, reader.FieldCount)
            .Select(i => ToReDataType(reader.GetDataTypeName(i)))
            .ToArray();

        await using var connection = await connectionService.GetConnectionAsync(ConnectionSource.DwhWrite, ct);
        if (connection is not NpgsqlConnection npgsqlConnection)
        {
            throw new InvalidOperationException("Р”Р»СЏ DWH С‚СЂРµР±СѓРµС‚СЃСЏ NpgsqlConnection.");
        }
        await using var transaction = await npgsqlConnection.BeginTransactionAsync(ct);

        var tableName = await CreateTableAsync(npgsqlConnection, transaction, types);
        
        await FillTableAsync(npgsqlConnection, reader, tableName, ct);

        await transaction.CommitAsync(ct);

        var entity = new DataConnectorEntity()
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            TableName = tableName,
            FieldList = aliases.Select((c, i) => new DataConnectorField()
            {
                Alias = c,
                Column = $"column{i + 1}",
                DataType = types[i],
                CanBeNull = true,
            }).ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        db.DataConnectors.Add(entity);
        await db.SaveChangesAsync(ct);
        
        return entity;
    }

    private static async Task<string> CreateTableAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        DataType[] types)
    {
        var tableId = Guid.NewGuid();
        string tableName = $"table_{tableId}";
    
        var createTableSql = $"""
                              CREATE TABLE "{tableName}" (
                                  rownum SERIAL PRIMARY KEY,
                                  {types.Select((t, i) => $"\"column{i + 1}\" {ToPostgresType(t)}").JoinBy(",\n")}
                              )
                              """;

        await using var command = new NpgsqlCommand(createTableSql, connection, transaction);
        await command.ExecuteNonQueryAsync();
        return tableName;
    }

    private static string ToPostgresType(DataType type)
    {
        return type switch
        {
            DataType.Text => "TEXT",
            DataType.Integer => "BIGINT",
            DataType.Number => "DOUBLE PRECISION",
            DataType.Bool => "BOOLEAN",
            DataType.DateTime => "TIMESTAMP",
        };
    }
    
    private static DataType ToReDataType(string type)
    {
        return type switch
        {
            "String" => DataType.Text,
            "Double" or "Decimal" => DataType.Number,
            "Int64" or "Int32" => DataType.Integer,
            "DateTime" => DataType.DateTime,
            "Boolean" => DataType.Bool,
        };
    }

    private static async Task FillTableAsync(
        NpgsqlConnection connection,
        DbDataReader reader,
        string tableName,
        CancellationToken ct)
    {
        var columns = Enumerable
            .Range(1, reader.FieldCount)
            .Select(i => $"\"column{i}\"")
            .JoinBy(", ");

        await using var writer = await connection
            .BeginBinaryImportAsync($"COPY \"{tableName}\" ({columns}) FROM STDIN (FORMAT BINARY)", ct);
        while (await reader.ReadAsync(ct))
        {
            await writer.StartRowAsync(ct);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                await writer.WriteAsync(reader.GetValue(i), ct);
            }
        }

        await writer.CompleteAsync(ct);
    }
}
