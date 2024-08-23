using System.Diagnostics;
using System.Globalization;
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
        ["or"] ="@or",
        ["="] = "@eq",
        ["!="] = "@neq",
        ["<"] = "@lt",
        ["<="] = "@leq",
        [">"] = "@gt",
        [">="] = "@geq",
    };
    
    public override IExpr VisitExpr(LangParser.ExprContext context)
    {
        if (context.children?.Count == 3)
        {
            if (context.children[0].GetText() == "(" && context.children[2].GetText() == ")")
            {
                return Visit(context.children[1]);
            }

            if (context.children[0] is LangParser.ExprContext && context.children[2] is LangParser.ExprContext)
            {
                var key = context.children[1].GetText();
                if (_binaryOperators.TryGetValue(key, out var name))
                {
                    return new FuncExpr()
                    {
                        Name = key,
                        Arguments = [Visit(context.children[0]), Visit(context.children[2])]
                    };
                }
            }
        }

        return base.VisitExpr(context);
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