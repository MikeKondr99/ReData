using System.Net.Mime;
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
    public static IReadOnlyList<FunctionDefinition> Functions { get; } =
        new IEnumerable<FunctionDefinition>[]
        {
            FunctionAnalyzer.GetFunctions(typeof(ImplicitConversionFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(StringFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(ConversionFunctions)),
            new ComparisonFunctions().GetFunctions(),
            FunctionAnalyzer.GetFunctions(typeof(NumberFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(MathFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(FinancialFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(ConditionalFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(LogicFunctions)),
            FunctionAnalyzer.GetFunctions(typeof(ReflectionFunctions)),
        }.SelectMany(f => f).ToArray();

    public static FunctionStorage GetFunctions(DatabaseTypeFlags database)
    {
        return new FunctionStorage(
            Functions.Select(f => new ReData.Query.Functions.FunctionDefinition()
            {
                Doc = f.Doc,
                Name = f.Name,
                Arguments = f.Arguments,
                Template = f.Templates.FirstOrDefault(t => t.Key.HasFlag(database)).Value,
                ReturnType = f.ReturnType,
                Kind = f.Kind,
                ImplicitCast = f.ImplicitCast,
                NullIf = f.NullIf
            }));
    }
}