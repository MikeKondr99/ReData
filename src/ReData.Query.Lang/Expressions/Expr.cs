using System.Text;
using Antlr4.Runtime;
using Pattern;
using Pattern.Unions;
using ReData.Query.Core;

namespace ReData.Query.Lang.Expressions;

public abstract record Expr
{
   public ExprSpan Span { get; init; }

    private int? _hash;
    public int Hash => _hash ??= GetHashCode();
    
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

    public override int GetHashCode()
    {
        return 17;
    }

    public bool Equivalent(Expr other)
    {
        return this.Hash == other.Hash;
    }
    
    public bool NotEquivalent(Expr other)
    {
        return this.Hash != other.Hash;
    }
    
    public Expr Replace(Expr pattern, Expr value)
    {
        if (this.Equivalent(pattern))
        {
            return value;
        }

        if (this is FuncExpr f)
        {
            return new FuncExpr()
            {
                Name = f.Name,
                Arguments = f.Arguments.Select(a => a.Replace(pattern, value)).ToArray(),
                Span = f.Span,
                Kind = f.Kind,
            };
        }
        return this;
    }
}