using ReData.Query.Core.Template;

namespace ReData.Query.Core.Components;

public interface IQuerySource
{
    public IResolvedTemplate? Name { get; }

    public IFieldStorage Fields();
}