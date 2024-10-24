namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class LogicFunctions 
{
    [Binary(BinaryOperation.And)]
    public static Ret<Bool> And(Bool left, Bool right) => new()
    {
        [All] = $"({left} AND {right})", 
    };
    
    [Binary(BinaryOperation.Or)]
    public static Ret<Bool> Or(Bool left, Bool right) => new()
    {
        [All] = $"({left} OR {right})", 
    };
    
    public static Ret<Bool> Not(Bool input) => new()
    {
        [All] = $"(NOT {input})", 
    };
}