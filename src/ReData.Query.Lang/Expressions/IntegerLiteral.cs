using System.Diagnostics.CodeAnalysis;

namespace ReData.Query.Lang.Expressions;

public sealed record IntegerLiteral : Literal<long>
{
    [SetsRequiredMembers]
    public IntegerLiteral(long value) : base(value) {}

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public new void Deconstruct(out long value)
    {
        value = Value;
    }
}