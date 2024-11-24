namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public class DateTimeFunctions
{
    
    public static Ret<DateTime?> Date(Text? input) => new()
    {
        [PostgreSql] = $"TO_TIMESTAMP({input}, 'YYYY-MM-DD HH24:MI')",
        [MySql] = $"STR_TO_DATE({input}, '%Y-%m-%d %H:%i')",
        [SqlServer] = $"CONVERT(DATETIME, {input}, 120)", // 120 corresponds to 'yyyy-mm-dd hh:mi:ss(24h)'
        [ClickHouse] = $"parseDateTimeBestEffort({input})",
        [Oracle] = $"TO_DATE({input}, 'YYYY-MM-DD HH24:MI')",
    };
    
    public static Ret<Integer?> Now() => new()
    {
    };
    
    public static Ret<DateTime?> Date(Integer? year, Integer? month, Integer day) => new()
    {
    };
    
    [Method]
    public static Ret<Integer?> Year(DateTime? input) => new()
    {
    };
    
    [Method]
    public static Ret<Integer?> Month(DateTime? input) => new()
    {
    };
    
    [Method]
    public static Ret<Integer?> Second(DateTime? input) => new()
    {
    };
}