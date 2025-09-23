using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Core.Template;

public record struct ResolvedExpr : IResolvedTemplate, IResolvedType
{
    public required ExprNode Node { get; init; }
    public required ITemplate Template { get; init; }
    public required ExprType Type { get; init; }
    public IReadOnlyList<ResolvedExpr>? Arguments { get; init; }
}

