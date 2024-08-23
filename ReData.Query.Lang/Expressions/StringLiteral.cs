namespace ReData.Query.Lang.Expressions;

public record StringLiteral(string Value) : ILiteral<string>;