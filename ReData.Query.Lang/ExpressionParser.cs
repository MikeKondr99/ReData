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

    public override IExpr VisitUnary(LangParser.UnaryContext context)
    {
        if (context.children is [TerminalNodeImpl op, LangParser.ExprContext expr])
        {
            return new FuncExpr()
            {
                Name = op.GetText(),
                Arguments = [VisitExpr(expr)]
            };
        }
        throw new Exception("Non valid unary expression");
    }

    public override IExpr VisitScope(LangParser.ScopeContext context)
    {
        return Visit(context.children[1]);
    }

    public override IExpr VisitBinary(LangParser.BinaryContext context)
    {
        if (context.children is [LangParser.ExprContext left, TerminalNodeImpl op, LangParser.ExprContext right])
        {
            return new FuncExpr()
            {
                Name = op.GetText(),
                Arguments = [Visit(left), Visit(right)]
            };
        }
        throw new Exception("Non valid binary expression");
    }

    public override IExpr VisitObjectFunction(LangParser.ObjectFunctionContext context)
    {
        var args = context.children.OfType<LangParser.ExprContext>().Select(a => Visit(a));
        return new FuncExpr()
        {
            Name = context.children[2].GetText(),
            Arguments = args.ToArray(),
            Kind = FuncExprKind.Method,
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
