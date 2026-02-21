using ReData.Query.Common;

namespace ReData.Query.Core.Template;

/// <summary>
/// Signals resolver-friendly expression error from dynamic template providers.
/// </summary>
public sealed class TemplateExprErrorException(ExprError error) : Exception(error.Message)
{
    public ExprError Error { get; } = error;
}
