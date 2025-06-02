using ReData.Query.Core.Template;

namespace ReData.Query.Core.Template;

public record struct ResolvedTemplate(ITemplate Template) : IResolvedTemplate
{
    public IReadOnlyList<ResolvedExpr>? Arguments => null;
}