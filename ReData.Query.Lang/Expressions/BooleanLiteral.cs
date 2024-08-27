namespace ReData.Query.Lang.Expressions;

public record struct BooleanLiteral(bool Value) : ILiteral<bool>;