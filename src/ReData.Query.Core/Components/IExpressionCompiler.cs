using System.Text;
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Components;

public interface IExpressionCompiler
{
    StringBuilder Compile(StringBuilder builder, IResolvedTemplate node);
}