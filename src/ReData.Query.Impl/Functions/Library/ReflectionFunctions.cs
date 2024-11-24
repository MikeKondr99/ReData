using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static BinaryOperation;
using static ConstKey;

public static class ReflectionFunctions
{
    
    [Method]
    public static Ret<Text> Type(Text input) => new()
    {
        [All] = $"'Text'",
    };
    
    [Method]
    public static Ret<Text> Type(Number input) => new()
    {
        [All] = $"'Number'",
    };
    
    [Method]
    public static Ret<Text> Type(Integer input) => new()
    {
        [All] = $"'Integer'",
    };
    
    [Method]
    public static Ret<Text> Type(Bool input) => new()
    {
        [All] = $"'Bool'",
    };
    
    [Method]
    public static Ret<Text> Type(Text? input) => new()
    {
        [All] = $"'Text?'",
    };
    
    [Method]
    public static Ret<Text> Type(Number? input) => new()
    {
        [All] = $"'Number?'",
    };
    
    [Method]
    public static Ret<Text> Type(Integer? input) => new()
    {
        [All] = $"'Integer?'",
    };
    
    [Method]
    public static Ret<Text> Type(Bool? input) => new()
    {
        [All] = $"'Bool?'",
    };
    
    [Method]
    public static Ret<Text> Type(Null input) => new()
    {
        [All] = $"'Null'",
    };
    
    [Method]
    public static Ret<Text> Type(Null? input) => new()
    {
        [All] = $"'Null'",
    };
    
    [Method]
    public static Ret<Text> Type(Unknown input) => new()
    {
        [All] = $"'Unknown'",
    };
    
    [Method]
    public static Ret<Text> Type(Unknown? input) => new()
    {
        [All] = $"'Unknown?'",
    };
    
}