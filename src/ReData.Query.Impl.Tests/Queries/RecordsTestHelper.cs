using ReData.Query.Core.Value;

namespace ReData.Query.Impl.Tests.Queries;

public static class RecordsTestHelper
{
    public static IEnumerable<Dictionary<string, IValue>> PrepareRecords(this IEnumerable<dynamic> objects)
    {
        return objects.Select(ConvertDynamicToIValueDictionary);
    }

    private static Dictionary<string, IValue> ConvertDynamicToIValueDictionary(dynamic dynamicObject)
    {
        if (dynamicObject == null)
        {
            throw new ArgumentNullException(nameof(dynamicObject));
        }

        var dictionary = new Dictionary<string, IValue>();

        // Handle ExpandoObject which is often used with dynamic
        if (dynamicObject is IDictionary<string, object> expandoDict)
        {
            foreach (var kvp in expandoDict)
            {
                dictionary[kvp.Key] = ConvertToIValue(kvp.Value);
            }
        }
        else
        {
            // Handle regular objects using reflection
            var properties = dynamicObject.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(dynamicObject);
                dictionary[property.Name] = ConvertToIValue(value);
            }
        }

        return dictionary;
    }

    private static IValue ConvertToIValue(object? value)
    {
        if (value is null)
        {
            return default(NullValue);
        }

        switch (value)
        {
            case string str:
                return new TextValue(str);
            case bool b:
                return new BoolValue(b);
            case int i:
                return new IntegerValue(i);
            case long l:
                return new IntegerValue(Convert.ToInt32(l)); // or handle as separate case if you have LongValue
            case double d:
                return new NumberValue(d);
            case float f:
                return new NumberValue(Convert.ToDouble(f));
            case decimal dec:
                return new NumberValue(Convert.ToDouble(dec));
            case DateTime dt:
                return new DateTimeValue(dt);
            default:
                throw new InvalidOperationException($"Unsupported type: {value.GetType().FullName}");
        }
    }
}