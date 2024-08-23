namespace ReData.Query.Lang.Expressions;

public sealed record IntegerLiteral(long Value) : ILiteral<long>;