using ReData.Query.Visitors;

namespace ReData.Query;

public interface IQuerySource
{
    public IResolvedTemplate? Name { get; }

    public IFieldStorage Fields();
}