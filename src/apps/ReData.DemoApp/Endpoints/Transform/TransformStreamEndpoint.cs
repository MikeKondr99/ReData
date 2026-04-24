using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using ReData.DemoApp.Database;
using ReData.DemoApp.Services;
using ReData.Query;

namespace ReData.DemoApp.Endpoints.Transform;

using FastEndpoints;
using Factory = ReData.Query.Factory;

/// <summary>
/// POC: потоковая выдача transform-данных через IAsyncEnumerable.
/// </summary>
public class TransformStreamEndpoint : Endpoint<TransformPocRequest, Results<Ok<TransformStreamResponse>, NotFound, InternalServerError>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public required DwhService DwhService { get; init; }

    public override void Configure()
    {
        Get("/transform/stream");
        Tags("Transform");
        AllowAnonymous();

        Description(d => d
            .Produces<TransformStreamResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Ok<TransformStreamResponse>, NotFound, InternalServerError>> ExecuteAsync(
        TransformPocRequest req,
        CancellationToken ct)
    {
        try
        {
            var connector = await Db.DataConnectors.FirstOrDefaultAsync(dc => dc.Id == req.ConnectorId, ct);

            if (connector is null)
            {
                return TypedResults.NotFound();
            }

            var queryBuilder = DwhService.GetQueryBuilder(connector.Id);
            var query = queryBuilder.Build();
            var total = await GetTotalAsync(queryBuilder, ct);
            var fields = query.Fields().Select(f => new TransformFieldViewModel
            {
                Alias = f.Alias,
                Type = f.Type.Type,
                CanBeNull = f.Type.CanBeNull,
            }).ToArray();

            var runner = Factory.CreateQueryExecuter(DatabaseType.PostgreSql);
            var connection = new NpgsqlConnection(DwhService.ReadConnection);
            var reader = await runner.GetDataReaderAsync(query, connection);

            return TypedResults.Ok(new TransformStreamResponse
            {
                Fields = fields,
                Total = total,
                Data = ReadRowsAsync(reader, connection, ct),
            });
        }
        catch
        {
            return TypedResults.InternalServerError();
        }
    }

    private async Task<long> GetTotalAsync(Query.Core.QueryBuilder queryBuilder, CancellationToken ct)
    {
        var totalQuery = queryBuilder
            .Select(new()
            {
                ["TOTAL"] = "COUNT()",
            })
            .Unwrap()
            .Build();

        var runner = Factory.CreateQueryExecuter(DatabaseType.PostgreSql);
        await using var connection = new NpgsqlConnection(DwhService.ReadConnection);
        await using var reader = await runner.GetDataReaderAsync(totalQuery, connection);

        if (!await reader.ReadAsync(ct))
        {
            return 0;
        }

        return Convert.ToInt64(reader.GetValue(0), CultureInfo.InvariantCulture);
    }

    private static async IAsyncEnumerable<object> ReadRowsAsync(
        DbDataReader reader,
        DbConnection connection,
        [EnumeratorCancellation] CancellationToken ct)
    {
        try
        {
            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>(reader.FieldCount);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i) is DBNull ? null : reader.GetValue(i);
                }

                yield return row;
            }
        }
        finally
        {
            await reader.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private static async IAsyncEnumerable<object> ReadNoRows()
    {
        await Task.CompletedTask;
        yield break;
    }
}
