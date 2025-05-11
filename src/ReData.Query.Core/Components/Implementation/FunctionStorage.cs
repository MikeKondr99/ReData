using System.Diagnostics;
using Pattern;
using Pattern.Unions;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components.Implementation;

public sealed class FunctionStorage : IFunctionStorage
{
    private ILookup<string, FunctionDefinition> _lookup;

    private string[] _allFunctionNames;

    private ILookup<FunctionArgumentType, FunctionDefinition> _implicitCasts;

    public FunctionStorage(IEnumerable<FunctionDefinition> functions)
    {
        functions = functions.Where(f => f.Template is not null).ToArray();
        _allFunctionNames = functions.Select(f => f.Name).Distinct().ToArray();
        _lookup = functions.Where(f => f.ImplicitCast is null).ToLookup(f => f.Name, f => f);
        _implicitCasts = functions.Where(f => f.ImplicitCast is not null).ToLookup(
            f => f.Arguments[0].Type,
            f => f
        );
    }

    private static bool ValidFunctionKind(FunctionKind defined, FunctionKind used)
    {
        return defined == used || (defined is FunctionKind.Method && used is FunctionKind.Default);
    }

    private IEnumerable<FunctionDefinition> GetValidFunctions(FunctionSignature sign)
    {
        var temp = _lookup[sign.Name];
        temp = temp
            .Where(f => f.Arguments.Count == sign.ArgumentTypes.Count)
            .Where(f => ValidFunctionKind(f.Kind, sign.Kind));
        return temp;
    }


    private FunctionDefinition? GetImplicit(FunctionArgumentType from, FunctionArgumentType to)
    {
        if (from == to)
            return new FunctionDefinition
            {
                Name = "",
                Doc = null,
                Arguments =
                [
                    new FunctionArgument
                    {
                        Name = "input",
                        Type = from,
                        PropagateNull = true,
                    }
                ],
                ReturnType = new FunctionReturnType()
                {
                    DataType = to.DataType,
                    CanBeNull = to.CanBeNull,
                    Aggregated = false,
                },
                Kind = FunctionKind.Default,
                Template = new Template.Template()
                {
                    Tokens = [new ArgToken(0)]
                },
                ImplicitCast = new ImplicitCastMetadata()
                {
                    Cost = 0,
                },
                ConstPropagation = Types.ConstPropagation.Default,
                CustomNullPropagation = null,
            };

        return _implicitCasts[from].FirstOrDefault(f =>
            f.ReturnType.DataType == to.DataType && f.ReturnType.CanBeNull == to.CanBeNull);
    }


    public Result<FunctionResolution, FunctionResolutionError> ResolveFunction(FunctionSignature sign)
    {
        var cons = ConstPropagation(sign.ArgumentTypes);
        var agg = AggPropagation(sign.ArgumentTypes);

        if (agg is not Option<bool>.Some(var aggr))
        {
            return new FunctionResolutionError("Не допускается комбинирование агрегированных и нет значений в аргументах функции");
        }
        
        var functions = GetValidFunctions(sign);

        List<FunctionResolution> matches = [];
        foreach (var func in functions)
        {
            var args = func.Arguments.Zip(sign.ArgumentTypes, (a, t) => GetImplicit(new FunctionArgumentType()
            {
                DataType = t.DataType,
                CanBeNull = t.CanBeNull
            }, a.Type));
            if (func.ReturnType.Aggregated && aggr)
            {
                return new FunctionResolutionError("Не допускается вложенность агрегирования");
            }
            var resolution = new FunctionResolution()
            {
                Function = func,
                PropagatesNull = PropagatesNull(func, sign),
                ReturnsConst = func.ConstPropagation switch
                {
                    Types.ConstPropagation.Default => cons,
                    Types.ConstPropagation.AlwaysTrue => true,
                    Types.ConstPropagation.AlwaysFalse => false,
                },
                ReturnsAggregated = func.ReturnType.Aggregated || aggr,
                Casts = (args as IEnumerable<FunctionDefinition>).ToArray()
            };
            if (!resolution.Casts.Contains(null))
            {
                matches.Add(resolution);
            }
        }

        var result = matches.MinBy(m => m.Casts.Sum(c => Math.Pow(10, c.ImplicitCast?.Cost ?? 0)));
        if (result is null)
        {
            return new FunctionResolutionError($"Функция {sign} не была найдена");
        }

        return result;
    }


    public static Option<bool> AggPropagation(IEnumerable<ExprType> aggs)
    {
        if (aggs.Any(t => t.Aggregated))
        {
            if (aggs.All(t => t.Aggregated || t.IsConstant))
            {
                return true;
            }
            return Option.None();
        } 
        return false;
    }

    public static bool ConstPropagation(IEnumerable<ExprType> types)
    {
        return types.All(at => at.IsConstant);
    }

    
    private bool PropagatesNull(FunctionDefinition function, FunctionSignature sign)
    {
        // Если не может быть null значит не может
        if (!function.ReturnType.CanBeNull) return false;

        // Если спец правило смотрим по нему
        if (function.CustomNullPropagation is not null)
        {
            return function.CustomNullPropagation(sign.ArgumentTypes.Select(a => a.CanBeNull));
        }

        // Если любой параметр прокидывает null и может быть null.
        for (int i = 0; i < function.Arguments.Count; i++)
        {
            if (function.Arguments[i].PropagateNull && sign.ArgumentTypes[i].CanBeNull)
            {
                return true;
            }
        }

        return false;
    }
}

public record struct FunctionResolutionError(string Message);

public record FunctionResolution
{
    public required FunctionDefinition Function { get; init; }

    public required FunctionDefinition[] Casts { get; init; }

    public required bool ReturnsConst { get; init; }

    public required bool ReturnsAggregated { get; init; }

    public required bool PropagatesNull { get; init; }
}