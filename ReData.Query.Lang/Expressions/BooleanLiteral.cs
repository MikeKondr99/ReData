namespace ReData.Query.Lang.Expressions;

public sealed record BooleanLiteral(bool Value) : ILiteral<bool>;