using Antlr4.Runtime;
using ReData.Query.Common;

namespace ReData.Query.Lang.Expressions;

public sealed class ErrorListener : IAntlrErrorListener<IToken>
{
    public void SyntaxError(
        TextWriter textWriter,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        throw new ExprErrorException(e)
        {
            Error = new ExprError()
            {
                Span = new ExprSpan((uint)line, (uint)charPositionInLine, 1000, 1000),
                Message = msg,
            }
        };
    }
}