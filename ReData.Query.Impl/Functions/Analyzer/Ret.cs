using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions;

public record Ret<T> : Ret;

public record Ret
{
    private Dictionary<DatabaseTypeFlags, ITemplate> _templates = new();

    public IReadOnlyDictionary<DatabaseTypeFlags, ITemplate> Templates => _templates;

    public TemplateInterpolatedStringHandler this[DatabaseTypeFlags db]
    {
        set => _templates[db] = value.Compile();
    }
}