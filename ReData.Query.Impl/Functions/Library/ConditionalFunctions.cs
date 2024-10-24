namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class ConditionalFunctions
{
        
    public static Ret<Text> If(Bool condition, Text then, Text @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
    };
    
    public static Ret<Integer> If(Bool condition, Integer then, Integer @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
    };
    
    public static Ret<Number> If(Bool condition, Number then, Number @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
    };
    
    public static Ret<Number> If(Bool condition, Bool then, Bool @else) => new()
    {
        [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
    };
}