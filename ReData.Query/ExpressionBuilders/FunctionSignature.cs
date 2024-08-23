namespace ReData.Query;

public sealed class FunctionSignature
{
    public FunctionSignature(string Name, IEnumerable<ExprType> Parameters)
    {
        this.Name = Name;
        this.Parameters = Parameters;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            foreach (var parameter in Parameters)
            {
                hash = hash * 23 + (parameter.GetHashCode());
            }
            return hash;
        }
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not FunctionSignature other)
            return false;

        if (Name != other.Name)
            return false;

        if (!Parameters.SequenceEqual(other.Parameters))
            return false;

        return true;
    }

    public string Name { get; init; }
    public IEnumerable<ExprType> Parameters { get; init; }

    public void Deconstruct(out string Name, out IEnumerable<ExprType> Parameters)
    {
        Name = this.Name;
        Parameters = this.Parameters;
    }
}