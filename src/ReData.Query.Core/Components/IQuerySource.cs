using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public interface IQuerySource
{
    public IResolvedTemplate? Name { get; }

    public IEnumerable<Field> Fields();
}