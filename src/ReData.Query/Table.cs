namespace ReData.Query;

public record struct Table : IQuerySource
{
    public Table(string name, IReadOnlyList<Query.Field> fields)
    {
        this.Name = name;
        _fields = new FieldStorage(fields);
    }

    public string Name { get; set; }
    private readonly IFieldStorage _fields;
    public IFieldStorage Fields(IFunctionStorage functions) => _fields;

}