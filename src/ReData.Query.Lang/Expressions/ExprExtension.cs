using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree.Xpath;
using Dunet;
using Pattern;
using ReData.Query.Core;

namespace ReData.Query.Lang.Expressions;

public sealed class TokenErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        throw new ExprErrorException(e)
        {
            Error = new ExprError()
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
        throw new ExprErrorException(e)
        {
            Error = new ExprError()
            {
                Span = new ExprSpan(line, charPositionInLine, offendingSymbol.StopIndex + 1 - offendingSymbol.StopIndex),
                Message = msg,
            }
        };
    }
}

public class ExprErrorException : Exception
{
    public required ExprError Error { get; init; }

    public ExprErrorException(Exception e) : base("", e) {}
}
