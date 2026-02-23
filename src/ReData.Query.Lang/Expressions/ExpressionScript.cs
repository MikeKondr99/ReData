namespace ReData.Query.Lang.Expressions;

public enum ScriptDeclarationKind
{
    Const,
    Macro,
}

public sealed record ScriptDeclaration
{
    public required ScriptDeclarationKind Kind { get; init; }

    public required string Name { get; init; }

    public required Expr Expression { get; init; }
}

public sealed record ExpressionScript
{
    public required IReadOnlyList<ScriptDeclaration> Declarations { get; init; }

    public IReadOnlyList<ScriptDeclaration> GetConstantDeclarations() =>
        Declarations.Where(x => x.Kind == ScriptDeclarationKind.Const).ToArray();

    public IReadOnlyList<ScriptDeclaration> GetMacroDeclarations() =>
        Declarations.Where(x => x.Kind == ScriptDeclarationKind.Macro).ToArray();

    public required Expr Expression { get; init; }
}
