using ReData.Query.Core.Template;

namespace ReData.Query.Core.Template;

public record struct TableTemplate(ITemplate Template) : IResolvedTemplate
{
    public IReadOnlyList<ResolvedExpr>? Arguments => null;
}