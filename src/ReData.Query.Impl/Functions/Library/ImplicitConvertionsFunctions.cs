using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class ImplicitConversionFunctions
{

    [Implicit(1)]
    public static Ret<Number?> NumberFromNull(Null? input) => ConversionFunctions.Num(input);

    [Implicit(1)]
    public static Ret<Integer?> IntegerFromNull(Null? input) => ConversionFunctions.Int(input);

    [Implicit(1)]
    public static Ret<Bool?> BoolFromNull(Null? input) => ConversionFunctions.Bool(input);

    [Implicit(1)]
    public static Ret<Text?> TextFromNull(Null? input) => ConversionFunctions.Text(input);
    
    [Implicit(1)]
    public static Ret<Number?> Optional(Number input) => new()
    {
        [All] = $"{input}",
        NullIf = (_) => true,
    };
    
    [Implicit(1)]
    public static Ret<Integer?> Optional(Integer input) => new()
    {
        [All] = $"{input}",
        NullIf = (_) => true,
    };
    
    [Implicit(1)]
    public static Ret<Bool?> Optional(Bool input) => new()
    {
        [All] = $"{input}",
        NullIf = (_) => true,
    };
    
    [Implicit(1)]
    public static Ret<Text?> Optional(Text input) => new()
    {
        [All] = $"{input}",
        NullIf = (_) => true,
    };

    [Implicit(2)]
    public static Ret<Number> ToNum(Integer input) => ConversionFunctions.Num(input).Cast<Number>();

    [Implicit(2)]
    public static Ret<Number?> ToNum(Integer? input) => ConversionFunctions.Num(input);

    [Implicit(2)]
    public static Ret<Number?> ToNum2(Integer input) => ConversionFunctions.Num(input);
}