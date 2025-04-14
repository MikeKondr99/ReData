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

    public Field this[string alias] => Fields.FirstOrDefault(f => f.Alias == alias);
    public Field this[int index] => Fields[index];
}

public sealed record FieldStorage(IReadOnlyList<Field> Fields) : IFieldStorage;