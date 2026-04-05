using System.Diagnostics.CodeAnalysis;
using ReData.Query.Common;

namespace ReData.Query.Lang.Expressions;

public sealed record StringLiteral : Literal<string>
{
    [SetsRequiredMembers]
    public StringLiteral(string value)
        : base(value)
    {
    }

    public StringLiteral(string value, ExprSpan span)
        : base(value)
    {
        Span = span;
    }
    
    public override string ToString()
    {
        return $"'{Value}'";
    }
    
    public new void Deconstruct(out string value)
    {
        value = Value;
    }
}