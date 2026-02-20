namespace ReData.Query.Lang.Expressions;

public sealed record ConstantDeclaration
{
    public required string Name { get; init; }

    public required Expr Expression { get; init; }
}

public sealed record ExpressionScript
{
    public required IReadOnlyList<ConstantDeclaration> Contants { get; init; }

    public required Expr Expression { get; init; }
}
