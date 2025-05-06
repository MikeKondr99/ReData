using Pattern;
using Pattern.Unions;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public interface IFieldStorage
{
    public IReadOnlyList<Field> Fields { get; }
    public Option<Field> this[string alias]
    {
        get
        {
            var field = Fields.FirstOrDefault(f => f.Alias == alias);
            return field.Alias is null ? Option.None() : Option.Some(field);
        }
    }

    public Option<Field> this[int index] => index >= 0 && index < Fields.Count ? Fields[index] : Option.None();
}