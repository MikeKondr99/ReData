using System.Dynamic;
using System.Reflection;
using ReData.Query.Functions;

namespace ReData.Query.Impl.Functions;

public static class FunctionAnalyzer
{

    public static IEnumerable<FunctionDefinition> GetFunctions(Type type)
    {
        var methods = type.GetMethods();
        foreach (var method in methods)
        {
            var func = GetFunction(method);
            if (func is not null)
            {
                yield return func;
            }
        }
    }
    
    private static FunctionDefinition? GetFunction(MethodInfo method)
    {
        // return type
        var ret = GetReturnType(method);
        if (ret is not var (retType, canReturnNull)) return null;
        // params
        var prs = GetArgumentsTypes(method);
        if (prs is null) return null;
        // templates
        object?[] args = new object[prs.Count];
        for (int i = 0; i < prs.Count; i++)
        {
            args[i] = Activator.CreateInstance(method.GetParameters()[i].ParameterType, args: [i]);
        }
        var templates = method.Invoke(null, args) as Ret;
        if (templates is null) return null;
        
        var doc = method.GetCustomAttribute<DocAttribute>()?.Text;
        var rename = method.GetCustomAttribute<FunctionNameAttribute>()?.Name;
        return new FunctionDefinition()
        {
            Name = rename ?? method.Name,
            Doc = doc,
            ReturnType = new FunctionReturnType()
            {
                DataType = retType,
                CanBeNull = canReturnNull,
            },
            Arguments = prs,
            Templates = templates.Templates,
        };
    }

    private static (DataType type, bool canReturnNull)? GetReturnType(MethodInfo method)
    {
        var ret = method.ReturnType;
        if (ret.IsAssignableTo(typeof(Ret)))
        {
            var args = ret.GetGenericArguments();
            if (args.Length == 1)
            {
                var retType = args[0];
                var canBeNull = UnwrapNullable(ref retType);
                return (TypeFromClrType(retType), canBeNull);
            }
        }
        return null;
    }

    static Type[] types = [typeof(Integer), typeof(Number), typeof(Text), typeof(Bool)];
    
    private static IReadOnlyList<FunctionArgument>? GetArgumentsTypes(MethodInfo method)
    {
        var result = new List<FunctionArgument>();
        var prs = method.GetParameters();
        foreach (var p in prs)
        {
            var pt = p.ParameterType;
            var canBeNull = UnwrapNullable(ref pt);
            if (!types.Contains(p.ParameterType)) return null;
            result.Add(new FunctionArgument()
            {
                Name = p.Name,
                Type = new FunctionArgumentType()
                {
                    DataType = TypeFromClrType(pt),
                    CanBeNull = canBeNull,
                }
            });
        }
        return result;
    }

    private static bool UnwrapNullable(ref Type type)
    {
        var inner = Nullable.GetUnderlyingType(type);
        if (inner is not null)
        {
            type = inner;
        }
        return inner is not null;
    }
    
    private static DataType TypeFromClrType(Type type)
    {
        if (type == typeof(Text)) return DataType.Text;
        if (type == typeof(Integer)) return DataType.Integer;
        if (type == typeof(Number)) return DataType.Number;
        if (type == typeof(Bool)) return DataType.Boolean;
        if (type == typeof(Null)) return DataType.Null;
        throw new Exception($"Не удалось определить тип данных по типу {type.Name}");
    }
}