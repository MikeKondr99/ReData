namespace ReData.Query.Core.Template;

public interface IToken;

public record struct ConstToken(string Text) : IToken;

public record struct ArgToken(int Index) : IToken;

