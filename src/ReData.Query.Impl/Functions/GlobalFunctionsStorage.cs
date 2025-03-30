using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using ReData.Query.Functions;
using ReData.Query.Impl.Functions.Library;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions;

using static ReData.Query.Impl.Functions.DatabaseTypeFlags;
using Signatures = IReadOnlyList<(IReadOnlyList<DataType> param, DataType returnType)>;
using static DataType;

public class GlobalFunctionsStorage
{
    private static Dictionary<DatabaseTypeFlags, FunctionStorage> storages = new Dictionary<DatabaseTypeFlags, FunctionStorage>();
        
    public static IReadOnlyList<FunctionDefinition> Functions { get; } =
        new FunctionsDescriptor[]
        {
            new ImplicitConversionFunctions(),
            new StringFunctions(),
            new DateFunctions(),
            new ConversionFunctions(),
            new ComparisonFunctions(),
            new NumberFunctions(),
            new MathFunctions(),
            new FinancialFunctions(),
            new ConditionalFunctions(),
            new LogicFunctions(),
            new ReflectionFunctions(),
        }.SelectMany(f => f.GetFunctions()).ToArray();
    

    public static FunctionStorage GetFunctions(DatabaseTypeFlags database)
    {
        if (storages.TryGetValue(database, out var storage))
        {
            return storage;
        }

        var newStorage = new FunctionStorage(
            Functions.Select(f => new ReData.Query.Functions.FunctionDefinition()
            {
                Doc = f.Doc,
                Name = f.Name,
                Arguments = f.Arguments,
                Template = f.Templates.FirstOrDefault(t => t.Key.HasFlag(database)).Value,
                ReturnType = f.ReturnType,
                Kind = f.Kind,
                ImplicitCast = f.ImplicitCast,
                CustomNullPropagation = f.CustomNullPropagation,
            }));
        storages[database] = newStorage;
        return newStorage;
    }
}