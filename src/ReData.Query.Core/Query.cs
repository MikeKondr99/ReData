using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core;

public sealed record Query : IQuerySource
{
    public IQuerySource From { get; init; } = default(NoSource);
    public IReadOnlyList<SelectItem>? Select { get; init; }
    public IReadOnlyList<ResolvedExpr>? Where { get; init; }
    public IReadOnlyList<OrderItem>? OrderBy { get; init; }
    public IReadOnlyList<ResolvedExpr>? GroupBy { get; init; }

    public uint Limit { get; init; }

    public uint Offset { get; init; }

    public required IResolvedTemplate Name { get; init; }

    public IFieldStorage Fields()
    {
        if (Select is null)
        {
            return From.Fields();
        }

        return new FieldStorage(Select.Select(m => new Field
        {
            Alias = m.Alias,
            Template = new ResolvedTemplate(Template.Template.Create($"{Name.Template.ToString()}.{m.Column.Template.ToString()}")).Template,
            Type = new FieldType(m.ResolvedExpr.Type.DataType, m.ResolvedExpr.Type.CanBeNull),
        }).ToArray());
    }
    
    private record struct NoSource : IQuerySource
    {
        public NoSource()
        {
        }

        public IResolvedTemplate? Name => null;
        public IFieldStorage Fields() => new FieldStorage([]);
    }
}