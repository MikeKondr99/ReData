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
        var rt = GetReturnType(method);
        if (rt is not var (retType, canReturnNull)) return null;
        // params
        var prs = GetArgumentsTypes(method);
        if (prs is null) return null;
        // templates
        object?[] args = new object[prs.Count];
        var methodParameters = method.GetParameters();
        for (int i = 0; i < prs.Count; i++)
        {
            args[i] = CreateArg(methodParameters[i].ParameterType, i);
        }
        var ret = method.Invoke(null, args) as Ret;
        if (ret is null) return null;
        
        var doc = method.GetCustomAttribute<DocAttribute>()?.Text;
        var isImplicit = method.GetCustomAttribute<ImplicitAttribute>();
        var rename = method.GetCustomAttribute<FunctionNameAttribute>()?.Name;
        var isBinary = method.GetCustomAttribute<BinaryAttribute>() is not null;
        var isUnary = method.GetCustomAttribute<UnaryAttribute>() is not null;
        var isMethod = method.GetCustomAttribute<MethodAttribute>() is not null;
        return new FunctionDefinition
        {
            Name = rename ?? method.Name,
            Doc = doc,
            ReturnType = new FunctionReturnType()
            {
                DataType = retType,
                CanBeNull = canReturnNull,
            },
            Kind = (isBinary,isUnary,isMethod) switch
            {
                (true,_,_) => FunctionKind.Binary,
                (_,true,_) => FunctionKind.Unary,
                (_,_,true) => FunctionKind.Method,
                _ => FunctionKind.Default,
            },
            Arguments = prs,
            Templates = ret.Templates,
            ImplicitCast = isImplicit is not null ? new ImplicitCastMetadata() 
            {
                Cost = isImplicit.Priority
            } : null,
            NullIf = ret.NullIf,

        };
    }

    private static object? CreateArg(Type type, int index)
    {
        var innerType = type;
        var nullable = UnwrapNullable(ref innerType);
        
        var instance = Activator.CreateInstance(innerType, args: [index]);

        if (nullable)
        {
            instance = Activator.CreateInstance(type, args: [instance]);
        }
        return instance;
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

    static Type[] types = [typeof(Integer), typeof(Number), typeof(Text), typeof(Bool), typeof(Null)];
    
    private static IReadOnlyList<FunctionArgument>? GetArgumentsTypes(MethodInfo method)
    {
        if (method.Name == "Coalesce")
        {
            int a = 5;
        }
        var result = new List<FunctionArgument>();
        var prs = method.GetParameters();
        foreach (var p in prs)
        {
            var pt = p.ParameterType;
            var canBeNull = UnwrapNullable(ref pt);
            if (!types.Contains(pt)) return null;
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
        if (type == typeof(DateTime)) return DataType.DateTime;
        throw new Exception($"Не удалось определить тип данных по типу {type.Name}");
    }
}