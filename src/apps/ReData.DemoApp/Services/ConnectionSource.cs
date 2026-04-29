namespace ReData.DemoApp.Services;

/// <summary>
/// Логический источник подключения к БД.
/// </summary>
public enum ConnectionSource
{
    /// <summary>
    /// Read-only подключение к DWH.
    /// </summary>
    DwhRead,

    /// <summary>
    /// Write-подключение к DWH.
    /// </summary>
    DwhWrite
}
