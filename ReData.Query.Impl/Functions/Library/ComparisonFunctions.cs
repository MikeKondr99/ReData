namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static BinaryOperation;

public static class ComparisonFunctions
{
    [Binary(LessThen)]
    public static Ret<Bool> Less(Integer left, Integer right) => new()
    {
        [All] = $"({left} < {right})", 
    };
    
    [Binary(LessOrEqual)]
    public static Ret<Bool> LessEqual(Integer left, Integer right) => new()
    {
        [All] = $"({left} <= {right})", 
    };
    
    [Binary(GreaterThen)]
    public static Ret<Bool> Greater(Integer left, Integer right) => new()
    {
        [All] = $"({left} > {right})", 
    };
    
    [Binary(GreaterOrEqual)]
    public static Ret<Bool> GreaterEqual(Integer left, Integer right) => new()
    {
        [All] = $"({left} >= {right})", 
    };
    
    // TODO: Not tested
    public static Ret<Bool> Between(Integer input, Integer min, Integer max) => new()
    {
        [All] = $"({input} BETWEEN {min} AND {max})",
    };
    
    [Binary(LessThen)]
    public static Ret<Bool> Less(Number left, Number right) => new()
    {
        [All] = $"({left} < {right})", 
    };
    
    [Binary(LessOrEqual)]
    public static Ret<Bool> LessEqual(Number left, Number right) => new()
    {
        [All] = $"({left} <= {right})", 
    };
    
    [Binary(GreaterThen)]
    public static Ret<Bool> Greater(Number left, Number right) => new()
    {
        [All] = $"({left} > {right})", 
    };
    
    [Binary(GreaterOrEqual)]
    public static Ret<Bool> GreaterEqual(Number left, Number right) => new()
    {
        [All] = $"({left} >= {right})", 
    };
    
    // TODO: Not tested
    public static Ret<Bool> Between(Number input, Number min, Number max) => new()
    {
        [All] = $"({input} BETWEEN {min} AND {max})",
    };
    
    
    [Binary(BinaryOperation.Equal)]
    public static Ret<Bool> Equal(Number left, Number right) => new()
    {
        [All] = $"({left} = {right})", 
    };
    
    [Binary(BinaryOperation.Equal)]
    public static Ret<Bool> Equal(Text left, Text right) => new()
    {
        [All] = $"({left} = {right})", 
    };
    
    [Binary(BinaryOperation.Equal)]
    public static Ret<Bool> Equal(Bool left, Bool right) => new()
    {
        [All] = $"({left} = {right})", 
    };
    
    [Binary(BinaryOperation.Equal)]
    public static Ret<Bool> Equal(Integer left, Integer right) => new()
    {
        [All] = $"({left} = {right})", 
    };
    
    [Binary(BinaryOperation.NotEqual)]
    public static Ret<Bool> NotEqual(Number left, Number right) => new()
    {
        [All] = $"({left} <> {right})", 
    };
    
    [Binary(BinaryOperation.NotEqual)]
    public static Ret<Bool> NotEqual(Text left, Text right) => new()
    {
        [All] = $"({left} <> {right})", 
    };
    
    [Binary(BinaryOperation.NotEqual)]
    public static Ret<Bool> NotEqual(Bool left, Bool right) => new()
    {
        [All] = $"({left} <> {right})", 
    };
    
    [Binary(BinaryOperation.NotEqual)]
    public static Ret<Bool> NotEqual(Integer left, Integer right) => new()
    {
        [All] = $"({left} <> {right})", 
    };
    
    public static Ret<Bool> IsNull(Integer? value) => new()
    {
        [All] = $"({value} IS NULL)", 
    };
    
    public static Ret<Bool> IsNull(Number? value) => new()
    {
        [All] = $"({value} IS NULL)", 
    };
    
    public static Ret<Bool> IsNull(Text? value) => new()
    {
        [All] = $"({value} IS NULL)", 
    };
}