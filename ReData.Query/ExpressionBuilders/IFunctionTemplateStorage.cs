using ReData.Query.Visitors;

namespace ReData.Query;

public interface IFunctionTemplateStorage
{
    public ITemplate GetTemplate(FunctionSignature sign);
}