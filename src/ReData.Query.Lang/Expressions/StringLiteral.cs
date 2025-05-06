using System.Diagnostics.CodeAnalysis;

namespace ReData.Query.Lang.Expressions;

public sealed record StringLiteral : Literal<string>
{
    [SetsRequiredMembers]
    public StringLiteral(string value) : base(value) {}
    
    public override string ToString()
    {
        return $"'{Value}'";
    }
    
    public void Deconstruct(out string value)
    {
        value = Value;
    }
}