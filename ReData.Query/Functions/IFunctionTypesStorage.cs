using System.Collections;
using System.Dynamic;
using System.Runtime.CompilerServices;
using ReData.Query.Visitors;

namespace ReData.Query;

public interface IFunctionTypesStorage
{
    public ExprType GetType(FunctionSignature sign);
}
