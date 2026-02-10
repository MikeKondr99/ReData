using ReData.Query.Core.Components.Implementation;
using ReData.Query.Impl.Functions.Library;
using ReData.Query.Core.Template;

namespace ReData.Query.Impl.Functions;


public class GlobalFunctionsStorage
{
    private static Dictionary<DatabaseTypes, FunctionStorage> storages = new Dictionary<DatabaseTypes, FunctionStorage>();
        
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
            new TrigonometryFunctions(),
            new ColorFunctions()
        }.SelectMany(f => f.GetFunctions()).ToArray();
    

    public static FunctionStorage GetFunctions(DatabaseTypes database)
    {
        if (storages.TryGetValue(database, out var storage))
        {
            return storage;
        }

        var newStorage = new FunctionStorage(
            Functions
                .Select(f =>
                {
                    var template = ResolveTemplate(f, database);
                    if (template is null)
                    {
                        return null;
                    }

                    return new ReData.Query.Core.Types.FunctionDefinition()
                    {
                        Doc = f.Doc,
                        Name = f.Name,
                        Arguments = f.Arguments,
                        Template = template,
                        ReturnType = f.ReturnType,
                        Kind = f.Kind,
                        ImplicitCast = f.ImplicitCast,
                        CustomNullPropagation = f.CustomNullPropagation,
                        ConstPropagation = f.ConstPropagation,
                    };
                })
                .OfType<ReData.Query.Core.Types.FunctionDefinition>());
        storages[database] = newStorage;
        return newStorage;
    }

    private static IFunctionTemplate? ResolveTemplate(FunctionDefinition definition, DatabaseTypes database)
    {
        return definition.Templates.FirstOrDefault(t => t.Key.HasFlag(database)).Value;
    }
}
