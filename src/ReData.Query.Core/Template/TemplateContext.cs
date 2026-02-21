using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;

namespace ReData.Query.Core.Template;

/// <summary>
/// Context for resolving dynamic function templates.
/// </summary>
public sealed record TemplateContext
{
    /// <summary>
    /// Fields of the current query source.
    /// </summary>
    public required IReadOnlyList<Field> Fields { get; init; }

    /// <summary>
    /// Resolved function arguments.
    /// </summary>
    public required IReadOnlyList<ResolvedExpr> Arguments { get; init; }

    /// <summary>
    /// Contants available in the resolution environment.
    /// </summary>
    public required IReadOnlyDictionary<string, IValue> Constants { get; init; }
}
