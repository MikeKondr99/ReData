using System.Text;
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Components;

public interface IExpressionCompiler
{
    StringBuilder Compile(StringBuilder builder, IResolvedTemplate res);


    public string Compile(IResolvedTemplate res)
    {
        StringBuilder sb = new StringBuilder();
        Compile(sb, res);
        return sb.ToString();
    }
}