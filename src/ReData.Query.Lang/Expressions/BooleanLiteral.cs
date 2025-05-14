using System.Diagnostics.CodeAnalysis;

namespace ReData.Query.Lang.Expressions;

public sealed record BooleanLiteral : Literal<bool>
{
    public BooleanLiteral(bool value) : base(value) {}
    public override string ToString()
    {
        return Value ? "true" : "false";
    }
    
    public void Deconstruct(out bool value)
    {
        value = Value;
    }

}