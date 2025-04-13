using ReData.Query.Visitors;

namespace ReData.Query;

public record struct TableQuerySource : IQuerySource
{
    public TableQuerySource(IResolvedTemplate name, IReadOnlyList<Field> fields)
    {
        this.Name = name;
        _fields = new FieldStorage(fields);
    }

    public IResolvedTemplate Name { get; set; }
    
    private readonly IFieldStorage _fields;
    public IFieldStorage Fields() => _fields;

}