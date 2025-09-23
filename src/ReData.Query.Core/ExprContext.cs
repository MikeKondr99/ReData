using ReData.Query.Common;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core;

public record ExprContext
{
    public required IQuerySource QuerySource { get; init; }

    public required IReadOnlyList<Field> Fields { get; init; }

    public required List<ExprError> Errors { get; init; }

    public bool HasErrors() => Errors.Count > 0;
}

public record struct ExprWithContext
{
    public required ResolvedExpr Expr { get; init; }

    public required ExprContext Context { get; init; }

    public ExprWithContext AddErrorLints(ReadOnlySpan<Func<ResolvedExpr, string?>> lints)
    {
        if (Context.HasErrors())
        {
            return this;
        }

        foreach (var lint in lints)
        {
            var error = lint(Expr);
            if (error is not null)
            {
                Context.Errors.Add(new ExprError()
                {
                    Span = Expr.Node.Span,
                    Message = "Выражение не может быть агрегированным"
                });
            }
        }

        return this;
    }
}