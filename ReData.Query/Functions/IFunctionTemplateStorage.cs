using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query;

public interface IFunctionStorage
{
    public FunctionDefinition GetFunction (FunctionSignature sign);
}