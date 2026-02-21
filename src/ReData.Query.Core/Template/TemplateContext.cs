using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;
using ReData.Query.Common;

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
    /// Constant arguments (literals or constants) for the function.
    /// </summary>
    public required IReadOnlyList<IValue?> Arguments { get; init; }

    /// <summary>
    /// Source spans for function arguments in the original expression.
    /// </summary>
    public required IReadOnlyList<ExprSpan> ArgumentSpans { get; init; }

    /// <summary>
    /// Contants available in the resolution environment.
    /// </summary>
    public required IReadOnlyDictionary<string, IValue> Constants { get; init; }
}
