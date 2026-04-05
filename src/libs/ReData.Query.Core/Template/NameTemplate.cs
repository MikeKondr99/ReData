using ReData.Query.Core.Template;

namespace ReData.Query.Core.Template;

public record NameTemplate(ITemplate Name) : IResolvedTemplate
{
    public ITemplate Template => Name;

    public IReadOnlyList<ResolvedExpr>? Arguments => null;
}