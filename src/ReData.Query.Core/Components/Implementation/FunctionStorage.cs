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

    private ILookup<FunctionArgumentType,FunctionDefinition> _implicitCasts;
    
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
                CustomNullPropagation = null,
            };
        
        return _implicitCasts[from].FirstOrDefault(f =>
            f.ReturnType.DataType == to.DataType && f.ReturnType.CanBeNull == to.CanBeNull);

    }
    

    public Option<FunctionResolution> ResolveFunction(FunctionSignature sign)
    {
        var functions = GetValidFunctions(sign);

        List<FunctionResolution> matches = [];
        foreach (var func in functions)
        {
            var args = func.Arguments.Zip(sign.ArgumentTypes, (a, t) => GetImplicit(t, a.Type));
            var resolution = new FunctionResolution()
            {
                Function = func,
                Casts = (args as IEnumerable<FunctionDefinition>).ToArray()
            };
            if (!resolution.Casts.Contains(null))
            {
                matches.Add(resolution);
            }
        }
        
        var result = matches.MinBy(m => m.Casts.Sum(c => Math.Pow(10,c.ImplicitCast?.Cost ?? 0)));
        if (result is null)
        {
            return Option.None();
        }

        return result;
    }
    
}

public record FunctionResolution
{
    public required FunctionDefinition Function { get; init; }
    
    public required FunctionDefinition[] Casts { get; init; }

    public IEnumerable<IToken> GetTokens()
    {
        foreach (var token in Function.Template.Tokens)
        {
            if (token is ConstToken) yield return token;
            else if (token is ArgToken(var index))
            {
                foreach (var castToken in Casts[index].Template.Tokens)
                {
                    if (castToken is ConstToken) yield return castToken;
                    else if (castToken is ArgToken) yield return token;
                    else throw new UnreachableException();
                }
            }
            else
            {
                throw new UnreachableException();
            }
        }
        
    }
    
}
