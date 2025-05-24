using ReData.Query.Core.Components.Implementation;
using ReData.Query.Impl.Functions.Library;

namespace ReData.Query.Impl.Functions;


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
            new AggregationFunctions(),
            new ReflectionFunctions(),
            new TrigonometryFunctions()
        }.SelectMany(f => f.GetFunctions()).ToArray();
    

    public static FunctionStorage GetFunctions(DatabaseTypeFlags database)
    {
        if (storages.TryGetValue(database, out var storage))
        {
            return storage;
        }

        var newStorage = new FunctionStorage(
            Functions.Select(f => new ReData.Query.Core.Types.FunctionDefinition()
            {
                Doc = f.Doc,
                Name = f.Name,
                Arguments = f.Arguments,
                Template = f.Templates.FirstOrDefault(t => t.Key.HasFlag(database)).Value,
                ReturnType = f.ReturnType,
                Kind = f.Kind,
                ImplicitCast = f.ImplicitCast,
                CustomNullPropagation = f.CustomNullPropagation,
                ConstPropagation = f.ConstPropagation,
            }));
        storages[database] = newStorage;
        return newStorage;
    }
}