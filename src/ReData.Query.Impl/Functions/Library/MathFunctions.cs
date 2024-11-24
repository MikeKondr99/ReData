namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static BinaryOperation;

public static class MathFunctions
{
    [Binary(Plus)]
    public static Ret<Integer?> Add(Integer? left, Integer? right) => new()
    {
        [All] = $"({left} + {right})", 
    };
    
    
    [Binary(Plus)]
    public static Ret<Number?> Add(Number? left, Number? right) => new()
    {
        [All] = $"({left} + {right})", 
    };
    
    [Binary(Minus)]
    public static Ret<Integer?> Sub(Integer? left, Integer? right) => new()
    {
        [All] = $"({left} - {right})", 
    };
    
    [Binary(Minus)]
    public static Ret<Number?> Sub(Number? left, Number? right) => new()
    {
        [All] = $"({left} - {right})", 
    };
    
    [Unary(UnaryOperation.Minus)]
    public static Ret<Integer?> UnaryMinus(Integer? value) => new()
    {
        [All] = $"(-{value})", 
    };
    
    [Unary(UnaryOperation.Minus)]
    public static Ret<Number?> UnaryMinus(Number? value) => new()
    {
        [All] = $"(-{value})", 
    };
    
    [Binary(Multiply)]
    public static Ret<Integer?> Mul(Integer? left, Integer? right) => new()
    {
        [All] = $"({left} * {right})", 
    };
    
    
    [Binary(Multiply)]
    public static Ret<Number?> Mul(Number? left, Number? right) => new()
    {
        [All] = $"({left} * {right})", 
    };
    
    [Binary(Divide)]
    public static Ret<Number?> Div(Number? left, Number? right) => new()
    {
        [All] = $"({left} / {right})", 
    };
    
    [Binary(Divide)]
    public static Ret<Integer?> Div(Integer? left, Integer? right) => new()
    {
        [PostgreSql | SqlServer] = $"({left} / {right})", 
        [MySql] = $"({left} DIV {right})", 
        [Oracle] = $"FLOOR({left} / {right})", 
        [ClickHouse] = $"intDiv({left}, {right})" 
    };
    
    
    
    [Method]
    public static Ret<Number?> Pow(Number? left, Number? right) => new()
    {
        [All] = $"POWER({left}, {right})", 
    };
    
    [Binary(Power)]
    public static Ret<Number?> PowOperator(Number? left, Number? right) => Pow(left, right);
    
    // TODO: NotTested
    public static Ret<Integer> E() => new()
    {
        [All] = $"2.718281828459045", 
    };
    
    // TODO: NotTested
    public static Ret<Integer> Pi() => new()
    {
        [All] = $"3.141592653589793", 
    };
}