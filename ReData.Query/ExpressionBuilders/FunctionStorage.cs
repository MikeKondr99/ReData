using System.Collections;
using System.Runtime.CompilerServices;
using ReData.Query.Visitors;

namespace ReData.Query;

[CollectionBuilder(typeof(FunctionStorageBuilder), "Create")]
public sealed class FunctionStorage : IFunctionTypesStorage, IFunctionTemplateStorage, IEnumerable<FunctionInfo>
{
    private Dictionary<FunctionSignature, FunctionInfo> _dictionary = new Dictionary<FunctionSignature, FunctionInfo>();
    
    public ExprType GetType(FunctionSignature sign)
    {
        if (_dictionary.TryGetValue(sign, out var func))
        {
            return func.ReturnType;
        }
        throw new KeyNotFoundException($"Function with {sign.Name}({String.Join(", ", sign.Parameters)} not found");
    }

    public ITemplate GetTemplate(FunctionSignature sign)
    {
        if (_dictionary.TryGetValue(sign, out var func))
        {
            return func.Template;
        }
        throw new KeyNotFoundException($"Function with {sign.Name}({String.Join(", ", sign.Parameters)} not found");
    }
    
    public static class FunctionStorageBuilder
    {
        public static FunctionStorage Create(ReadOnlySpan<FunctionInfo> values) => new FunctionStorage()
        {
            _dictionary = values.ToArray().ToDictionary(x => new FunctionSignature(x.Name,x.Parameters), x => x)
        };
    }

    public IEnumerator<FunctionInfo> GetEnumerator() => _dictionary.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.Values.GetEnumerator();
}