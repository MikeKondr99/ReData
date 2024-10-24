namespace ReData.Query.Visitors;

public interface IToken;

public record struct ConstToken(string Text) : IToken;

public record struct ArgToken(int Index) : IToken;

