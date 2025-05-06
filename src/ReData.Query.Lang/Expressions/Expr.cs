using Antlr4.Runtime;
using Pattern.Unions;
using ReData.Query.Core;

namespace ReData.Query.Lang.Expressions;

public abstract record Expr
{
    public ExprSpan Span { get; init; }
    
    public static Result<Expr,ExprError> Parse(string s)
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
            Expr expr = new ExpressionParser().VisitStart(parser.start());
            return Result.Ok(expr);
        }
        catch (ExprErrorException e)
        {
            return e.Error;
        }
        catch (Exception e)
        {
            return new ExprError()
            {
                Span = new ExprSpan(1, 1, 100),
                Message = e.Message
            };
        }
    }
}