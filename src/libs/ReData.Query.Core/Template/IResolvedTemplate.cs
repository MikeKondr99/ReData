namespace ReData.Query.Core.Template;

public interface IResolvedTemplate
{
    public ITemplate Template { get; }
    public IReadOnlyList<ResolvedExpr>? Arguments { get; }
}