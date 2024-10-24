using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree.Xpath;

namespace ReData.Query.Lang.Expressions;

public static class Expr
{
    public static IExpr Parse(string s)
    {
        var chars = new AntlrInputStream(s);
        var lexer = new LangLexer(chars);
        lexer.AddErrorListener(new TokenErrorListener());
        var tokens = new CommonTokenStream(lexer);
        tokens.Fill();
        var test = tokens.GetTokens();
        var parser = new LangParser(tokens);
        parser.AddErrorListener(new ErrorListener());
        var expr = new ExpressionParser().VisitStart(parser.start());
        return expr;
    }
    
}

public class TokenErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        throw new UnexpectedTokenException(e);
    }
}

public class ErrorListener : IAntlrErrorListener<IToken>
{
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        var expected = msg;
        if (msg.Contains("missing"))
        {
            expected = msg[
                (msg.IndexOf("missing ", StringComparison.Ordinal) + 8)
                ..(msg.IndexOf(" at ", StringComparison.Ordinal))];
        } else if (msg.Contains("expecting"))
        {
            expected = msg[(msg.IndexOf("expecting ", StringComparison.Ordinal) + 10)..];
        }
        
        if (expected == "{'(', '-', BOOLEAN, 'null', NAME, BLOCKED_NAME, STRING, INTEGER, NUMBER}")
        {
            expected = "expression";
        }  else if (expected == "<EOF>")
        {
            expected = "end of expression";
        }

        if (expected is ['{', ..var list, '}'])
        {
            var split = list.Split(", ");
            expected = String.Join(" or ", split);
        }
        
        
        throw new ParseException(e)
        {
            Line = line,
            Column = charPositionInLine,
            Length = offendingSymbol.StopIndex + 1 - offendingSymbol.StartIndex,
            Token = offendingSymbol.Text,
            Expected = expected,
        };
    }
}

public class ParseException : Exception
{
    public ParseException() {}

    public ParseException(Exception e) : base("", e) { }
    public override string Message => $"expected {Expected}";

    public required int Line { get; init; }
    
    public required int Column { get; init; }
    
    public required int Length { get; init; }
    
    public required string Expected { get; init; }
    
    public required string Token { get; init; }
}

public class UnexpectedTokenException : Exception
{
    public UnexpectedTokenException() {}

    public UnexpectedTokenException(Exception e) : base("", e) { }
    public override string Message => $"unexpected token";
}