namespace ReData.Query.Core.Template;

/// <summary>
/// Function template that can build a SQL template based on context.
/// </summary>
public interface IFunctionTemplate
{
    /// <summary>
    /// Gets a template for the provided context.
    /// </summary>
    /// <param name="context">Template evaluation context.</param>
    /// <returns>Resolved template.</returns>
    ITemplate GetTemplate(TemplateContext context);
}

/// <summary>
/// Static function template that does not depend on context.
/// </summary>
/// <param name="Template">Template instance.</param>
public sealed record StaticFunctionTemplate(ITemplate Template) : IFunctionTemplate
{
    /// <inheritdoc />
    public ITemplate GetTemplate(TemplateContext context) => Template;
}

/// <summary>
/// Dynamic function template computed from context.
/// </summary>
/// <param name="Provider">Template provider callback.</param>
public sealed record DynamicFunctionTemplate(Func<TemplateContext, ITemplate> Provider) : IFunctionTemplate
{
    /// <inheritdoc />
    public ITemplate GetTemplate(TemplateContext context) => Provider(context);
}
