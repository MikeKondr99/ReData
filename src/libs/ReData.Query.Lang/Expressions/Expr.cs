using System.Diagnostics;
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Pattern.Unions;
using ReData.Query.Common;

namespace ReData.Query.Lang.Expressions;

public abstract record Expr
{
    private static ActivitySource activitySource = new ActivitySource("ReData.Query.Lang");

    
   public ExprSpan Span { get; init; }

    private int? hash;
    public int Hash => hash ??= GetHashCode();
    
    public static Result<Expr, ExprError> Parse(string s)
    {
        var cacheKey = $"expr::{s}";
        if (Global.MemoryCache.TryGetValue<Expr>(cacheKey, out var result) && result is not null)
        {
            return result;
        }
        
        using var act = activitySource.StartActivity("expression parsing");
        act?.SetTag("expression", s);
        try
        {
            var chars = new AntlrInputStream(s);
            var lexer = new LangLexer(chars);
            lexer.AddErrorListener(new TokenErrorListener());
            var tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            var parser = new LangParser(tokens);
            parser.AddErrorListener(new ErrorListener());
            Expr expr = new ExpressionParser().VisitStart(parser.start());
            // Cache.TryAdd(s, expr);
            Global.MemoryCache.GetOrCreate<Expr>(cacheKey, (c) =>
            {
                c.SlidingExpiration = TimeSpan.FromMinutes(30);
                return expr;
            });
            return Result.Ok(expr);
        }
        catch (ExprErrorException e)
        {
            act?.SetStatus(ActivityStatusCode.Error);
            act?.AddException(e);
            return e.Error;
        }
        catch (Exception e)
        {
            act?.SetStatus(ActivityStatusCode.Error);
            act?.AddException(e);
            return new ExprError()
            {
                Span = new ExprSpan(1, 1, 100, 100),
                Message = e.Message
            };
        }
    }

    public static Result<ExpressionScript, ExprError> ParseScript(string s)
    {
        var cacheKey =  $"script::{s}";
        if (Global.MemoryCache.TryGetValue<ExpressionScript>(cacheKey, out var result) && result is not null)
        {
            return result;
        }

        using var act = activitySource.StartActivity("expression script parsing");
        act?.SetTag("expression", s);
        try
        {
            var chars = new AntlrInputStream(s);
            var lexer = new LangLexer(chars);
            lexer.AddErrorListener(new TokenErrorListener());
            var tokens = new CommonTokenStream(lexer);
            tokens.Fill();
            var parser = new LangParser(tokens);
            parser.AddErrorListener(new ErrorListener());
            var script = new ExpressionParser().VisitScript(parser.start());
            Global.MemoryCache.GetOrCreate<ExpressionScript>(cacheKey, (c) =>
            {
                c.SlidingExpiration = TimeSpan.FromMinutes(30);
                return script;
            });
            return Result.Ok(script);
        }
        catch (ExprErrorException e)
        {
            act?.SetStatus(ActivityStatusCode.Error);
            act?.AddException(e);
            return e.Error;
        }
        catch (Exception e)
        {
            act?.SetStatus(ActivityStatusCode.Error);
            act?.AddException(e);
            return new ExprError()
            {
                Span = new ExprSpan(1, 1, 100, 100),
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
        return Hash == other.Hash;
    }
    
    public bool NotEquivalent(Expr other)
    {
        return Hash != other.Hash;
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

    public static string Field(string alias)
    {
        alias = $"[{alias.Replace("]", @"\]")}]";
        return alias;
    }
}
