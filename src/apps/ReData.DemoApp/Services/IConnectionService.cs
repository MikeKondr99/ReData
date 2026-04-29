using System.Data.Common;

namespace ReData.DemoApp.Services;

/// <summary>
/// Выдает открытые подключения к БД.
/// Жизненным циклом подключений управляет вызывающий код.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Возвращает новое открытое подключение для указанного источника.
    /// Вызывающий код обязан закрыть и освободить подключение.
    /// </summary>
    Task<DbConnection> GetConnectionAsync(
        ConnectionSource source,
        CancellationToken ct = default);
}
