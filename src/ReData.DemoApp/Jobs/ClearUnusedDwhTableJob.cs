using Npgsql;
using ReData.DemoApp.Database;
using ReData.DemoApp.Services;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Enums;

namespace ReData.DemoApp.Jobs;

public sealed class ClearUnusedDwhTableJob
{
    private DwhService DwhService { get; init; }
    private ApplicationDatabaseContext Db { get; init; }

    public ClearUnusedDwhTableJob(
        ApplicationDatabaseContext db,
        DwhService dwhService
    )
    {
        DwhService = dwhService;
        Db = db;
    }

    [TickerFunction("Clear unused DWH tables")]
    public async Task ClearUnusedDwhTables(
        TickerFunctionContext context,
        CancellationToken ct)
    {
        var connectedTables = Db.DataConnectors.Select(dc => dc.TableName);
        var connectionString = DwhService.WriteConnection;

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);

        // Get all tables from public schema
        const string getTablesQuery =
            """
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE'
            """;

        var allTables = new List<string>();
        await using (var command = new NpgsqlCommand(getTablesQuery, connection))
        await using (var reader = await command.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                allTables.Add(reader.GetString(0));
            }
        }

        // Find tables to delete (not in connectedTables)
        var tablesToDelete = allTables.Except(connectedTables).ToList();

        // Delete the unused tables
        foreach (var tableName in tablesToDelete)
        {
            var dropTableQuery = $"DROP TABLE IF EXISTS public.\"{tableName}\"";

            try
            {
                await using var dropCommand = new NpgsqlCommand(dropTableQuery, connection);
                await dropCommand.ExecuteNonQueryAsync(ct);
            }
            catch (Exception ex)
            {
                // Logger.LogError(ex, "Failed to drop table: {TableName}", tableName);
                // Continue with other tables even if one fails
            }
        }

        // context.Logger.LogInformation("Completed clearing unused DWH tables. Dropped {Count} tables",
        //     tablesToDelete.Count);
    }
}