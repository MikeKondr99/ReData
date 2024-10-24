using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ReData.Query.Impl.Functions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class FunctionNameAttribute(string name) : Attribute
{
    public string Name => name;
}

public class UnaryAttribute(UnaryOperation operation) : FunctionNameAttribute(operation switch
{
    UnaryOperation.Minus => "-",
});

public enum UnaryOperation
{
    Minus = 1,
}
