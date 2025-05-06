using Dunet;
using Pattern;
using Pattern.Unions;
using ReData.Query.Core;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public interface IFunctionStorage
{
    public Option<FunctionResolution> ResolveFunction(FunctionSignature sign);
}
