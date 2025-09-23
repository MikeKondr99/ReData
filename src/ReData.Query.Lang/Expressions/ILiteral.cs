using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ReData.Query.Lang.Expressions;

public abstract record Literal : ExprNode
{
    
}

public abstract record Literal<T>(T Value) : Literal;