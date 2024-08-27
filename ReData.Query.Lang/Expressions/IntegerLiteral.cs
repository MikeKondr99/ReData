namespace ReData.Query.Lang.Expressions;

public record struct IntegerLiteral(long Value) : ILiteral<long>;