namespace ReData.Query.Lang.Expressions;

public record NumberLiteral(double Value) : ILiteral<double>;