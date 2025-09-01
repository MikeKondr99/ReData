using System.Runtime.InteropServices;
using ReData.Query.Core.Components;
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

    public IEnumerable<Field> Fields()
    {
        if (Select is null)
        {
            return From.Fields();
            // IToken[] nameTemplate = [..Name.Template.Tokens];
            // return new FieldStorage(From.Fields().Fields.Select(f => new Fiel.Fieldsd
            // {
            //     Alias = f.Alias,
            //     Type = f.Type,
            //     Template = Template.Template.FromTokens(),
            // }).ToArray());
        }

        return Select.Select(m => new Field
        {
            Alias = m.Alias,
            Template = m.Column.Template,
            Type = new FieldType(m.ResolvedExpr.Type.DataType, m.ResolvedExpr.Type.CanBeNull),
        });
    }
    
    private record struct NoSource : IQuerySource
    {
        public NoSource()
        {
        }

        public IResolvedTemplate? Name => null;
        public IEnumerable<Field> Fields() => [];
    }
}