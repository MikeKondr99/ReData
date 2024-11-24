namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class NumberFunctions
{
        
    [Method]
    public static Ret<Integer?> Mod(Integer? input, Integer? modulus) => new()
    {
        [All & ~SqlServer] = $"MOD({input}, {modulus})", 
        [SqlServer] = $"{input} % {modulus}" 
    };
    
    [Method]
    public static Ret<Integer?> Rem(Integer? input, Integer? modulus) => new()
    {
        [All & ~SqlServer] = $"MOD({input}, {modulus})", 
        [SqlServer] = $"{input} % {modulus}" 
    };
    
    public static Ret<Integer?> Abs(Integer? input) => new()
    {
        [All] = $"ABS({input})" 
    };
    
    public static Ret<Number?> Abs(Number? input) => new()
    {
        [All] = $"ABS({input})" 
    };

    public static Ret<Number?> Floor(Number? input) => new()
    {
        [All] = $"FLOOR({input})"
    };
    
    public static Ret<Number?> Ceil(Number? input) => new()
    {
        [All & ~SqlServer] = $"CEIL({input})", 
        [SqlServer] = $"CEILING({input})" 
    };

    public static Ret<Number?> Round(Number? input) => new()
    {
        [All &~ SqlServer &~ ClickHouse] = $"ROUND({input})", 
        [SqlServer] = $"ROUND(CAST({input} AS NUMERIC),0)", 
        [ClickHouse] = $"ROUND(CAST({input},'Decimal64(6)'),0)" 
    };
    
    public static Ret<Number?> Floor(Number? input, Number? step) => new()
    {
        [All] = $"FLOOR({input} / {step}) * {step}", 
    };
    
    public static Ret<Number?> Ceil(Number? input, Number? step) => new()
    {
        [All & ~Oracle] = $"CEILING({input} / {step}) * {step}", 
        [Oracle] = $"CEIL({input} / {step}) * {step}" 
    };

    public static Ret<Number?> Round(Number? input, Number? step) => new()
    {
        [All &~ SqlServer &~ ClickHouse] = $"ROUND({input} / {step}) * {step}", 
        [SqlServer] = $"ROUND({input} / {step}, 0) * {step}", 
        [ClickHouse] = $"ROUND(CAST({input} / {step},'Decimal64(6)')) * {step}", 
    };
    
    public static Ret<Number?> Floor(Number? input, Number? step, Number? offset) => new()
    {
        [All] = $"FLOOR(({input} - {offset}) / {step}) * {step} + {offset}", 
    };
    
    public static Ret<Number?> Ceil(Number? input, Number? step, Number? offset) => new()
    {
        [All & ~Oracle] = $"CEILING(({input} - {offset}) / {step}) * {step} + {offset}", 
        [Oracle] = $"CEIL(({input} - {offset}) / {step}) * {step} + {offset}", 
    };

    public static Ret<Number?> Round(Number? input, Number? step, Number? offset) => new()
    {
        [All &~ SqlServer &~ ClickHouse] = $"Round(({input} - {offset}) / {step}) * {step} + {offset}", 
        [SqlServer] = $"Round(({input} - {offset}) / {step}, 0) * {step} + {offset}", 
        [ClickHouse] = $"Round(CAST(({input} - {offset}) / {step},'Decimal64(6)')) * {step} + {offset}",
    };
    
    
    public static Ret<Bool?> Even(Integer? input) => new()
    {
        [All &~SqlServer] = $"(MOD({input}, 2) = 0)", 
        [SqlServer] = $"(({input} % 2) = 0)", 
    };
    
    public static Ret<Bool?> Odd(Integer? input) => new()
    {
        [All &~SqlServer] = $"(MOD({input}, 2) <> 0)", 
        [SqlServer] = $"(({input} % 2) <> 0)", 
    };
    
    [Method]
    public static Ret<Integer?> Sign(Integer? input) => new()
    {
        [All] = $"SIGN({input})", 
    };
    
    [Method]
    public static Ret<Integer?> Sign(Number? input) => new()
    {
        [All] = $"SIGN({input})", 
    };
    
    [Method]
    public static Ret<Bool?> Frac(Number? input) => new()
    {
        [All &~SqlServer] = $"MOD({input}, 1)", 
        [SqlServer] = $"({input} % 1)", 
    };
}