using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public interface INameResolver
{
    public TableTemplate ResolveName(ReadOnlySpan<string> path);
}