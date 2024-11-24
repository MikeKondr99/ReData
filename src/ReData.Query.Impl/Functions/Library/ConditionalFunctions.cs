namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class ConditionalFunctions
{
        
    public static Ret<Text?> If(Bool? condition, Text? then, Text? @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
        NullIf = (args) => args[1] || args[2],
    };
    
    public static Ret<Integer?> If(Bool? condition, Integer? then, Integer? @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
        NullIf = (args) => args[1] || args[2],
    };
    
    public static Ret<Number?> If(Bool? condition, Number? then, Number? @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
        NullIf = (args) => args[1] || args[2],
    };
    
    public static Ret<Bool?> If(Bool? condition, Bool? then, Bool? @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
        NullIf = (args) => args[1] || args[2],
    };
    
    [Method]
    public static Ret<Integer?> Or(Integer? input, Integer? alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
        NullIf = (args) => args.All(a => a),
    };
    
    public static Ret<Number?> Or(Number? input, Number? alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
        NullIf = (args) => args.All(a => a),
    };
    
    public static Ret<Text?> Or(Text? input, Text? alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
        NullIf = (args) => args.All(a => a),
    };
    
    public static Ret<Bool?> Or(Bool? input, Bool? alt) => new()
    {
        [All] = $"COALESCE({input}, {alt})", 
        NullIf = (args) => args.All(a => a),
    };
}