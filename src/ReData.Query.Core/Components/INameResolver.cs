using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public interface INameResolver
{
    public ResolvedTemplate ResolveName(ReadOnlySpan<string> path);
}