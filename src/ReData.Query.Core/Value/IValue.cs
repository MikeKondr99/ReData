using System.Globalization;
using Microsoft.Extensions.Primitives;
using ReData.Query.Core.Value;

namespace ReData.Query.Runners.Value;

public interface IValue
{
}

public static class ValueExtensions
{
    public static string? Text(this Dictionary<string,IValue> value, string key) => value[key] switch
    {
        TextValue(var v) => v,
        NullValue => null,
    };
    
    public static double? Num(this Dictionary<string,IValue> value, string key) => value[key] switch
    {
        NumberValue(var v) => v,
        NullValue => null,
    };
    
    public static long? Int(this Dictionary<string,IValue> value, string key) => value[key] switch
    {
        IntegerValue(var v) => v,
        NullValue => null,
    };

    public static string ToReDataLiteral(this IValue value) => value switch
    {
        IntegerValue(var v) => v.ToString(CultureInfo.InvariantCulture),
        NumberValue(var v) => v.ToString("0.0#############", CultureInfo.InvariantCulture),
        BoolValue(var v) => v ? "true" : "false",
        NullValue => "null",
        TextValue(var v) => $"'{v}'", // TODO Escaping
        DateTimeValue(var v) => $"Date({v.ToString("u", CultureInfo.InvariantCulture)})",
    };


}