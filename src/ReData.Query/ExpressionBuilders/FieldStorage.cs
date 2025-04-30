using Pattern.Unions;
using ReData.Query.Visitors;

namespace ReData.Query;


public record struct Field
{
    public required string Alias { get; init; }

    public required FieldType Type { get; init; }
}

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

public sealed record FieldStorage(IReadOnlyList<Field> Fields) : IFieldStorage;