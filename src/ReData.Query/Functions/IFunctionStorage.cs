using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query;

public interface IFunctionStorage
{
    public FunctionResolution? ResolveFunction(FunctionSignature sign);
}