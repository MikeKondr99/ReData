using System.Runtime.InteropServices;
using System.Runtime.Loader;
using ReData.Core;

namespace ReData.Query.Impl.Functions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class FunctionNameAttribute(string name) : Attribute
{
    public string Name => name;
}

public class UnaryAttribute(UnaryOperation operation) : FunctionNameAttribute(operation switch
{
    UnaryOperation.Minus => "-",
    var unknown => throw new UnknownEnumValueException<UnaryOperation>(unknown)
});

public enum UnaryOperation
{
    Minus = 1,
}
