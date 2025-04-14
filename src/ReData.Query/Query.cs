using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query;

public sealed record Query : IQuerySource
{
    public IQuerySource From { get; init; } = new NoSource();

    public IReadOnlyList<Map>? Select { get; init; }

    public IReadOnlyList<Node>? Where { get; init; }

    public IReadOnlyList<Order>? OrderBy { get; init; }

    public uint? Limit { get; init; }

    public uint Offset { get; init; }

    public record struct Order(Node Node, Order.Type Direction)
    {
        public enum Type
        {
            Asc,
            Desc
        }
    }
    
    public record struct Map(string Alias, IResolvedTemplate Template,  Node Node);

    public required IResolvedTemplate Name { get; init; }

    public IFieldStorage Fields()
    {
        if (this.Select is null) return From.Fields();
        return new FieldStorage(this.Select.Select(m => new Field
        {
            Alias = m.Alias,
            Type = new FieldType(m.Node.Type.Type, m.Node.Type.CanBeNull),
        }).ToArray());
    }
    
    private record struct NoSource() : IQuerySource
    {
        public IResolvedTemplate? Name => null;
        public IFieldStorage Fields() => new FieldStorage([]);
    }
}