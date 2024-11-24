namespace ReData.Query;

public interface IFieldStorage
{
    public IReadOnlyList<Query.Field> Fields { get; }
    
    public ExprType GetType(string fieldName)
    {
        var results = Fields.Where(x => x.Name == fieldName).ToArray();
        if (results.Length == 0)
        {
            throw new KeyNotFoundException($"Field with name `{fieldName}` not found");
        }
        return results[0].Type;
    }
}

public sealed class FieldStorage(IReadOnlyList<Query.Field> fields) : IFieldStorage
{
    public IReadOnlyList<Query.Field> Fields => fields;
    
    public override string ToString()
    {
        return String.Join(", ", fields.Select(f => $"{f.Name}:{f.Type}"));
    }
}

public sealed class EmptyFieldStorage() : IFieldStorage
{
    public IReadOnlyList<Query.Field> Fields => [];
}