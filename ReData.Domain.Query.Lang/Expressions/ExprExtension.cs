using Antlr4.Runtime;

namespace ReData.Domain.Query.Lang.Expressions;

public static class Expr
{
    public static IExpr Parse(string s)
    {
        var chars = new AntlrInputStream(s);
        var lexer = new LangLexer(chars);
        var tokens = new CommonTokenStream(lexer);
        var parser = new LangParser(tokens);
        var expr = new ExpressionParser().VisitExpr(parser.expr());
        return expr;
    }
    
}