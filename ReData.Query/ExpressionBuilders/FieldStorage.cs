namespace ReData.Query;

public interface IFieldStorage
{
    public ExprType GetType(string fieldName);

    public ExprType GetType(int index);
}

public sealed class FieldStorage(IReadOnlyList<Query.Field> fields) : IFieldStorage
{
    public ExprType GetType(string fieldName)
    {
        var results = fields.Where(x => x.Name == fieldName).ToArray();
        if (results.Length == 0)
        {
            throw new KeyNotFoundException($"Field with name `{fieldName}` not found");
        }
        return results[0].Type;
    }

    public ExprType GetType(int index)
    {
        return fields[index].Type;
    }

    public override string ToString()
    {
        return String.Join(", ", fields.Select(f => $"{f.Name}:{f.Type}"));
    }
}

public sealed class EmptyFieldStorage() : IFieldStorage
{
    public ExprType GetType(string fieldName)
    {
        throw new Exception($"It's EmptyFieldStorage there is no field with name {fieldName}");
    }

    public ExprType GetType(int index)
    {
        throw new Exception($"It's EmptyFieldStorage there is no field with index {index}");
    }
}