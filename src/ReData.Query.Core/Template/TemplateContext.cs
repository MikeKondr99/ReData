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
    /// Constant arguments (literals or variables) for the function.
    /// </summary>
    public required IReadOnlyList<IValue?> Arguments { get; init; }

    /// <summary>
    /// Variables available in the resolution environment.
    /// </summary>
    public required IReadOnlyDictionary<string, IValue> Variables { get; init; }
}
