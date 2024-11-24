using System.Collections;
using System.Runtime.CompilerServices;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions;

public record Ret<T> : Ret
{

    public Ret<T2> Cast<T2>()
    {
        var res = new Ret<T2>
        {
            NullIf = NullIf,
            _templates = _templates
        };
        return res;
    }
}

public record Ret
{
    protected Dictionary<DatabaseTypeFlags, ITemplate> _templates = new();

    public IReadOnlyDictionary<DatabaseTypeFlags, ITemplate> Templates => _templates;

    public TemplateInterpolatedStringHandler this[DatabaseTypeFlags db]
    {
        set => _templates[db] = value.Compile();
    }
    
    // public ConstTemplate Const { get; init; }
    
    public Func<bool[],bool>? NullIf { get; init; }
    
    
}


public enum ConstKey
{
    Const
}

// public struct ConstTemplate : IEnumerable
// {
//     public object Func { get; private set; }
//
//     public IEnumerator GetEnumerator()
//     {
//         throw new NotImplementedException();
//     }
//
//     public void Add(Func<string, string> func)
//     {
//         Func = func;
//     }
//     
//     public void Add(Func<int, string> func)
//     {
//         Func = func;
//     }
//     
// }
