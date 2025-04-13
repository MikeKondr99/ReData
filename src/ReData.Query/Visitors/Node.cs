using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface IResolvedTemplate
{
    public ITemplate Template { get; }
    public IReadOnlyList<Node>? Arguments { get; }
}

public interface IResolvedType
{
    public ExprType Type { get; }
}

public record NameTemplate(ITemplate Name) : IResolvedTemplate
{
    public ITemplate Template => Name;

    public IReadOnlyList<Node>? Arguments => null;
}


public record struct Node : IResolvedTemplate, IResolvedType
{
    public required IExpr Expression { get; init; }
    public required ITemplate Template { get; init; }
    public required ExprType Type { get; init; }
    public IReadOnlyList<Node>? Arguments { get; init; }
}