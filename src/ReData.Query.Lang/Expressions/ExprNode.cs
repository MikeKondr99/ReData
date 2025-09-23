using Antlr4.Runtime;
using Pattern.Unions;
using ReData.Query.Common;

namespace ReData.Query.Lang.Expressions;

public abstract record ExprNode
{
   public ExprSpan Span { get; init; }

    private int? hash;
    public int Hash => hash ??= GetHashCode();
    
    public static Result<ExprNode, ExprError> Parse(string s)
    {
        try
        {
            var chars = new AntlrInputStream(s);
            var lexer = new LangLexer(chars);
            lexer.AddErrorListener(new TokenErrorListener());
            var tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            var parser = new LangParser(tokens);
            parser.AddErrorListener(new ErrorListener());
            ExprNode exprNode = new ExpressionParser().VisitStart(parser.start());
            return Result.Ok(exprNode);
        }
        catch (ExprErrorException e)
        {
            return e.Error;
        }
        catch (Exception e)
        {
            return new ExprError()
            {
                Span = new ExprSpan(1, 1, 100,100),
                Message = e.Message
            };
        }
    }

    public override int GetHashCode()
    {
        return 17;
    }

    public bool Equivalent(ExprNode other)
    {
        return Hash == other.Hash;
    }
    
    public bool NotEquivalent(ExprNode other)
    {
        return Hash != other.Hash;
    }
    
    public ExprNode Replace(ExprNode pattern, ExprNode value)
    {
        if (this.Equivalent(pattern))
        {
            return value;
        }

        if (this is FuncExprNode f)
        {
            return new FuncExprNode()
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