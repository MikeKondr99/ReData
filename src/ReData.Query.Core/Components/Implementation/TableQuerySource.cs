using ReData.Common;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Core.Components.Implementation;

public record struct TableQuerySource : IQuerySource
{
    public TableQuerySource(IResolvedTemplate name, IReadOnlyList<Field> fields)
    {
        this.Name = name;
        this.fields = new FieldStorage(fields);
    }

    public IResolvedTemplate Name { get; set; }
    
    private readonly IFieldStorage fields;
    public IFieldStorage Fields() => fields;

}
