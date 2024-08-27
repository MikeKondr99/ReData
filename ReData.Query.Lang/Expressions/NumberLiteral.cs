namespace ReData.Query.Lang.Expressions;

public record struct NumberLiteral(double Value) : ILiteral<double>;