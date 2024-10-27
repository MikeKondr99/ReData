namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class ConversionFunctions
{
    public static Ret<Integer> Int(Text input) => new()
    {
        [SqlServer] = $"CAST({input} AS INTEGER)",
        [MySql] = $"CAST({input} AS SIGNED)",
        [PostgreSql | Oracle] = $"CAST({input} AS INTEGER)",
        [ClickHouse] = $"CAST({input} AS Int64)",
    };

    public static Ret<Integer> Int(Bool input) => new()
    {
        [All] = $"CASE WHEN {input} THEN 1 ELSE 0 END",
    };

    public static Ret<Integer> Int(Number input) => new()
    {
        [SqlServer] = $"CAST({input} AS INTEGER)",
        [PostgreSql | Oracle] = $"CAST(FLOOR({input}) AS INTEGER)",
        [MySql] = $"CAST(FLOOR({input}) AS SIGNED)",
        [ClickHouse] = $"CAST({input} AS Int64)"
    };

    public static Ret<Integer> Int(Integer input) => new()
    {
        [All] = $"{input}",
    };
    
    public static Ret<Integer?> Int(Null? input) => new()
    {
        [All] = $"{input}",
    };

    public static Ret<Number> Num(Text input) => new()
    {
        [All & ~ClickHouse] = $"CAST({input} AS DECIMAL)",
        [ClickHouse] = $"toDecimal64({input},10)"
    };

    public static Ret<Number> Num(Bool input) => new()
    {
        [All] = $"CASE WHEN {input} THEN 1.0 ELSE 0.0 END"
    };

    public static Ret<Number> Num(Number input) => new()
    {
        [All] = $"{input}"
    };

    public static Ret<Number> Num(Integer input) => new()
    {
        [All & ~ClickHouse] = $"CAST({input} AS DECIMAL)",
        [ClickHouse] = $"toDecimal64({input},10)"
    };
    
    public static Ret<Number?> Num(Null? input) => new()
    {
        [All] = $"{input}",
    };

    public static Ret<Bool> Bool(Text input) => new()
    {
        [SqlServer] = $"LEN({input}) > 0",
        [MySql | PostgreSql | ClickHouse | Oracle] = $"LENGTH({input}) > 0"
    };

    public static Ret<Bool> Bool(Bool input) => new()
    {
        [All] = $"{input}"
    };

    public static Ret<Bool> Bool(Number input) => new()
    {
        [All] = $"({input} > 0.0)"
    };

    public static Ret<Bool> Bool(Integer input) => new()
    {
        [All] = $"({input} > 0)"
    };
    
    public static Ret<Bool?> Bool(Null? input) => new()
    {
        [All] = $"{input}",
    };

    public static Ret<Text> Text(Text input) => new()
    {
        [All] = $"{input}"
    };

    public static Ret<Text> Text(Bool input) => new()
    {
        [All] = $"CASE WHEN {input} THEN 'true' ELSE 'false' END"
    };

    public static Ret<Text> Text(Number input) => new()
    {
        [All & ~ (MySql | Oracle)] = $"CAST({input} AS VARCHAR)",
        [MySql] = $"CAST({input} AS CHAR)",
        [Oracle] = $"REPLACE(TO_CHAR({input}),',','.')"
    };

    public static Ret<Text> Text(Integer input) => new()
    {
        [All & ~MySql & ~Oracle] = $"CAST({input} AS VARCHAR)",
        [MySql] = $"CAST({input} AS CHAR)",
        [Oracle] = $"TO_CHAR({input})"
    };
    
    public static Ret<Text?> Text(Null? input) => new()
    {
        [All] = $"{input}",
    };
    
    public static Ret<Integer> Coalesce(Integer? input, Integer alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
    };
    
    public static Ret<Number> Coalesce(Number? input, Number alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
    };
    
    public static Ret<Text> Coalesce(Text? input, Text alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
    };
    
    public static Ret<Bool> Coalesce(Bool? input, Bool alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
    };
    

}