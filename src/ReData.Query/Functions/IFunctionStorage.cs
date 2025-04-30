using Dunet;
using Pattern.Unions;
using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query;

public interface IFunctionStorage
{
    public Result<FunctionResolution, FunctionResolutionError> ResolveFunction(FunctionSignature sign);
}

[Union]
public abstract partial record FunctionResolutionError
{
    public sealed partial record FunctionNameNotFound(string Name, string? Suggestion);
    public sealed partial record FunctionSignatureNotFound(FunctionSignature Name, IEnumerable<FunctionSignature> Suggestion);
    public sealed partial record FunctionIsNotMethod(string Name);
}