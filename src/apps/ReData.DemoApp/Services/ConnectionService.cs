using System.Data;
using System.Data.Common;
using Npgsql;

namespace ReData.DemoApp.Services;

/// <summary>
/// Фабрика подключений к DWH.
/// Возвращает новые открытые подключения, которыми владеет вызывающий код.
/// </summary>
public sealed class ConnectionService(IConfiguration configuration) : IConnectionService
{
    private readonly string dwhReadConnection = 
        configuration.GetConnectionString("DwhRead")
        ?? configuration.GetConnectionString("dwh") 
        ?? string.Empty;
    
    private readonly string dwhWriteConnection = 
        configuration.GetConnectionString("DwhWrite") 
        ?? configuration.GetConnectionString("dwh") 
        ?? string.Empty;
        

    /// <inheritdoc />
    public async Task<DbConnection> GetConnectionAsync(ConnectionSource source, CancellationToken ct = default)
    {
        var connection = CreateConnection(source);
        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        return connection;
    }

    private DbConnection CreateConnection(ConnectionSource source)
    {
        return source switch
        {
            ConnectionSource.DwhRead => new NpgsqlConnection(dwhReadConnection),
            ConnectionSource.DwhWrite => new NpgsqlConnection(dwhWriteConnection),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестный источник подключения.")
        };
    }
}
