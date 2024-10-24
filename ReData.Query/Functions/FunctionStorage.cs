using System.Collections;
using System.Runtime.CompilerServices;
using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query;

public sealed class FunctionStorage : IFunctionStorage
{
    private ILookup<string, FunctionDefinition> _lookup;
    
    public FunctionStorage(IEnumerable<FunctionDefinition> functions)
    {
        functions = functions.Where(f => f.Template is not null);
        _lookup = functions.ToLookup(f => f.Name, f => f);
    }
    
    public FunctionDefinition GetFunction(FunctionSignature sign)
    {
        var funcs = _lookup[sign.Name];
        foreach (var func in funcs)
        {
            if (func.Arguments.Count != sign.ArgumentTypes.Count) continue;
            for (int i = 0; i < func.Arguments.Count; i++)
            {
                if(func.Arguments[i].Type.DataType != sign.ArgumentTypes[i].DataType) continue;
                if(!func.Arguments[i].Type.CanBeNull && sign.ArgumentTypes[i].CanBeNull) continue;
                return func;
            }
        }
        throw new Exception($"Function {sign} not found");
    }
}