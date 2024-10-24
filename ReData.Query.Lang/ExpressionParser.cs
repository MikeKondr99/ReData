using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime.Tree;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang;

internal class ExpressionParser : LangBaseVisitor<IExpr>
{
    private Dictionary<string, string> _binaryOperators = new()
    {
        ["+"] = "@add",
        ["-"] = "@sub",
        ["*"] = "@mul",
        ["/"] = "@div",
        ["and"] = "@and",
        ["or"] = "@or",
        ["="] = "@eq",
        ["!="] = "@neq",
        ["<"] = "@lt",
        ["<="] = "@leq",
        [">"] = "@gt",
        [">="] = "@geq",
    };

    private Dictionary<string, string> _UnaryOperators = new()
    {
        ["-"] = "@un_sub",
    };



    public override IExpr VisitStart(LangParser.StartContext context)
    {
        return Visit(context.children[0]);
    }

    public override IExpr VisitExpr(LangParser.ExprContext context)
    {
        return context.children switch
        {
            // Скобки ( expr )
            [TerminalNodeImpl, LangParser.ExprContext expr, TerminalNodeImpl] => VisitExpr(expr),
            // Бинарное expr + expr
            [LangParser.ExprContext left, TerminalNodeImpl op, LangParser.ExprContext right] =>
                new FuncExpr()
                {
                    Name = op.GetText(),
                    Arguments = [VisitExpr(left), VisitExpr(right)]
                },
            // Унарное -expr
            [TerminalNodeImpl op, LangParser.ExprContext expr] =>
                new FuncExpr()
                {
                    Name = op.GetText(),
                    Arguments = [VisitExpr(expr)]
                },
            // Остальное
            _ => base.VisitExpr(context),
        };
    }

    public override IExpr VisitName(LangParser.NameContext context)
    {
        var name = context.GetText();
        if (name[0] is '[' && name[^1] is ']')
        {
            name = name[1..^1];
        }

        name = name.Trim();

        return new NameExpr(name);
    }


    public override IExpr VisitString(LangParser.StringContext context)
    {
        return new StringLiteral(context.GetText().Trim(['\'']));
    }

    public override IExpr VisitInteger(LangParser.IntegerContext context)
    {
        return new IntegerLiteral(long.Parse(context.GetText()));
    }

    public override IExpr VisitNumber(LangParser.NumberContext context)
    {
        return new NumberLiteral(double.Parse(context.GetText(), CultureInfo.InvariantCulture));
    }

    public override IExpr VisitFunc(LangParser.FuncContext context)
    {
        var args = context.children.OfType<LangParser.ExprContext>().Select(a => Visit(a));
        return new FuncExpr()
        {
            Name = context.children[0].GetText(),
            Arguments = args.ToArray(),
        };
    }

    public override IExpr VisitBoolean(LangParser.BooleanContext context)
    {
        return new BooleanLiteral(bool.Parse(context.GetText()));
    }

    public override IExpr VisitNull(LangParser.NullContext context)
    {
        return new NullLiteral();
    }
}
