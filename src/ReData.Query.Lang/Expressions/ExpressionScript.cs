namespace ReData.Query.Lang.Expressions;

public sealed record VariableDeclaration
{
    public required string Name { get; init; }

    public required Expr Expression { get; init; }
}

public sealed record ExpressionScript
{
    public required IReadOnlyList<VariableDeclaration> Variables { get; init; }

    public required Expr Expression { get; init; }
}
