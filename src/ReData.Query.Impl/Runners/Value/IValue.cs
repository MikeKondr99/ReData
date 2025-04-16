using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices.JavaScript;
using ReData.Query.Impl.Runners.Value;

namespace ReData.Query.Impl.Runners;

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
    

}