namespace ReData.Query.Impl.Functions;

[Flags]
public enum DatabaseTypeFlags
{
    PostgreSql = 1,
    SqlServer = 2,
    MySql = 4,
    ClickHouse = 8,
    Oracle = 16,
    // Не забыть при добавлении поменять ALL
    All = 31
}