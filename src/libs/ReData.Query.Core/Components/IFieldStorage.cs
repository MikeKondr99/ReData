using Pattern.Unions;
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components;

public static class FieldStorageExtensions
{
    public static Option<Field> Get(this IEnumerable<Field> fields, string alias)
    {
        var field = fields.FirstOrDefault(f => f.Alias == alias);
        return field.Alias is null ? Option.None() : Option.Some(field);
    }
    
    public static Field Get(this IEnumerable<Field> fields, int index)
    {
        return fields.Skip(index).First();
    }
}