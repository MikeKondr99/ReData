using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static BinaryOperation;
using static ConstKey;

public static class StringFunctions
{
    [Doc("Retrieves a substring from input. The substring starts at a specified character position and continues to the end of the string.")]
    public static Ret<Text?> Substring(Text? input, Integer? start) => new()
    {
        [PostgreSql | MySql] = $"SUBSTRING({input} FROM {start})",
        [SqlServer] = $"SUBSTRING({input}, {start}, LEN({input}) - ({start} - 1))",
        [ClickHouse] = $"SUBSTRING({input}, {start}, LENGTH({input}) - ({start} - 1))",
        [Oracle] = $"SUBSTR({input}, {start})",
    };
    
    [Doc("Retrieves a substring from `input`. The substring starts at a specified character position and has a specified length.")]
    public static Ret<Text?> Substring(Text? input, Integer? start, Integer? count) => new()
    {
        [PostgreSql | MySql] = $"SUBSTRING({input} FROM {start} FOR {count})",
        [SqlServer | ClickHouse] = $"SUBSTRING({input}, {start}, {count})",
        [Oracle] = $"SUBSTR({input}, {start}, {count})"
    };

    [Method]
    public static Ret<Text?> Lower(Text? input) => new()
    {
        [All] = $"LOWER({input})",
    };
    
    [Method]
    public static Ret<Text?> Upper(Text? input) => new()
    {
        [All] = $"Upper({input})",
    };
    
    [Method]
    public static Ret<Text?> Trim(Text? input) => new()
    {
        [All] = $"TRIM({input})",
    };
    
    [Method]
    public static Ret<Text?> TrimLeft(Text? input) => new()
    {
        [All] = $"LTRIM({input})",
    };
    
    [Method]
    public static Ret<Text?> TrimRight(Text? input) => new()
    {
        [All] = $"RTRIM({input})",
    };
    
    [Method]
    public static Ret<Text?> Reverse(Text? input) => new()
    {
        [All] = $"REVERSE({input})"
    };
    
    [Binary(Plus)]
    public static Ret<Text?> Add(Text? left, Text? right) => new()
    {
        [SqlServer] = $"({left} + {right})",
        [PostgreSql] = $"({left} || {right})",
        [MySql | Oracle | ClickHouse] = $"CONCAT({left}, {right})",
    };
    
    // TODO: NotTested
    [Method]
    public static Ret<Text?> EmptyIsNull(Text? input) => new()
    {
        [All] = $"CASE WHEN {0} = '' THEN NULL ELSE {0} END", 
        NullIf = (_) => true,
    };
    
    
    
}