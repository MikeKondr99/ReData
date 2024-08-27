namespace ReData.Query;

public struct FunctionSignature
{
    public FunctionSignature(string Name, IReadOnlyList<ExprType> Parameters)
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
            for (int i = 0; i < Parameters.Count; i++)
            {
                hash = hash * 23 + (Parameters[i].GetHashCode());
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
    public IReadOnlyList<ExprType> Parameters { get; init; }

    public void Deconstruct(out string Name, out IEnumerable<ExprType> Parameters)
    {
        Name = this.Name;
        Parameters = this.Parameters;
    }
}