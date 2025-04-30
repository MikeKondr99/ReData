using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree.Xpath;
using Dunet;
using Pattern.Unions;

namespace ReData.Query.Lang.Expressions;

public record struct ExprSpan(int line, int column, int length);

[Union]
public partial record ParsingError
{
    public sealed partial record UnexpectedToken
    {
        public required ExprSpan Span { get; init; }
        public required IToken Token { get; init; }
        public required string Message { get; init; }
        public required string Expected { get; init; }
    }

    public sealed partial record UnrecognizedSymbol
    {
        public required ExprSpan Span { get; init; }
        public required string Message { get; init; }
    }
}



public static class Expr
{
    public static Result<IExpr,ParsingError> Parse(string s)
    {
        try
        {
            var chars = new AntlrInputStream(s);
            var lexer = new LangLexer(chars);
            lexer.AddErrorListener(new TokenErrorListener());
            var tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            var test = tokens.GetTokens();
            var parser = new LangParser(tokens);
            parser.AddErrorListener(new ErrorListener());
            IExpr expr = new ExpressionParser().VisitStart(parser.start());
            return Result.Ok(expr);
        }
        catch (UnexpectedTokenException e)
        {
            return e.Error;
        }
        catch (UnrecognizedSymbolException e)
        {
            return e.Error;
        }
    }
    
}

public sealed class TokenErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        throw new UnrecognizedSymbolException(e)
        {
            Error = new ParsingError.UnrecognizedSymbol()
            {
                Span = new ExprSpan(line, charPositionInLine, 1),
                Message = msg,
            }
        };
    }
}

public sealed class ErrorListener : IAntlrErrorListener<IToken>
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
        
        
        throw new UnexpectedTokenException(e)
        {
            Error = new ParsingError.UnexpectedToken()
            {
                Span = new ExprSpan(line,charPositionInLine,offendingSymbol.StopIndex + 1 - offendingSymbol.StartIndex),
                Message = msg,
                Token = offendingSymbol,
                Expected = expected,
            },
        };
    }
}

public class UnexpectedTokenException : Exception
{
    public required ParsingError.UnexpectedToken Error { get; init; }

    public UnexpectedTokenException(Exception e) : base("", e) {}
}

public class UnrecognizedSymbolException : Exception
{
    public required ParsingError.UnrecognizedSymbol Error { get; init; }

    public UnrecognizedSymbolException(Exception e) : base("", e) {}
}
