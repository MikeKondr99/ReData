namespace ReData.Query;

public interface IQuerySource
{
    public string? Name { get; }

    public IFieldStorage Fields(IFunctionStorage functions);
}