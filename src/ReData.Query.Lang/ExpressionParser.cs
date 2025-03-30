using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang;

internal class ExpressionParser : LangBaseVisitor<IRawExpr>
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
        ["^"] = "@hat",
    };

    private Dictionary<string, string> _UnaryOperators = new()
    {
        ["-"] = "@un_sub",
    };



    public override IRawExpr VisitStart(LangParser.StartContext context)
    {
        return Visit(context.children[0]);
    }

    public override IRawExpr VisitUnary(LangParser.UnaryContext context)
    {
        if (context.children is [TerminalNodeImpl op, LangParser.ExprContext expr])
        {
            return new FuncRawExpr()
            {
                Name = op.GetText(),
                Arguments = [VisitExpr(expr)],
                Kind = FuncExprKind.Unary,
            };
        }
        throw new Exception("Non valid unary expression");
    }

    public override IRawExpr VisitScope(LangParser.ScopeContext context)
    {
        return Visit(context.children[1]);
    }

    public override IRawExpr VisitBinary(LangParser.BinaryContext context)
    {
        if (context.children is [LangParser.ExprContext left, TerminalNodeImpl op, LangParser.ExprContext right])
        {
            return new FuncRawExpr()
            {
                Name = op.GetText(),
                Arguments = [Visit(left), Visit(right)],
                Kind = FuncExprKind.Binary,
            };
        }
        throw new Exception("Non valid binary expression");
    }

    public override IRawExpr VisitObjectFunction(LangParser.ObjectFunctionContext context)
    {
        var args = context.children.OfType<ParserRuleContext>().Select(a => Visit(a));
        return new FuncRawExpr()
        {
            Name = context.children[2].GetText(),
            Arguments = args.ToArray(),
            Kind = FuncExprKind.Method,
        };
    }

    public override IRawExpr VisitName(LangParser.NameContext context)
    {
        var name = context.GetText();
        if (name[0] is '[' && name[^1] is ']')
        {
            name = name[1..^1];
        }

        name = Regex.Replace(name, @"\\\]", "]");
        
        return new NameRawExpr(name);
    }


    public override IRawExpr VisitString(LangParser.StringContext context)
    {
        var value = context.GetText()[1..^1];
        value = Regex.Replace(value, @"\\(.)", m =>
        {
            char escapedChar = m.Groups[1].Value[0];

            return escapedChar switch
            {
                'r' => "\r",
                'n' => "\n",
                't' => "\t",
                '\'' => "'",
                '\\' => "\\",
                _ => m.Value // If it's not one of the recognized characters, return it unchanged
            };
        });
        return new StringLiteral(value);
    }

    public override IRawExpr VisitInteger(LangParser.IntegerContext context)
    {
        return new RawIntegerLiteral(long.Parse(context.GetText()));
    }

    public override IRawExpr VisitNumber(LangParser.NumberContext context)
    {
        return new RawNumberLiteral(double.Parse(context.GetText(), CultureInfo.InvariantCulture));
    }

    public override IRawExpr VisitFunc(LangParser.FuncContext context)
    {
        var args = context.children.OfType<LangParser.ExprContext>().Select(a => Visit(a));
        return new FuncRawExpr()
        {
            Name = context.children[0].GetText(),
            Arguments = args.ToArray(),
            Kind = FuncExprKind.Default,
        };
    }

    public override IRawExpr VisitBoolean(LangParser.BooleanContext context)
    {
        return new RawBooleanLiteral(bool.Parse(context.GetText()));
    }

    public override IRawExpr VisitNull(LangParser.NullContext context)
    {
        return new RawNullRawLiteral();
    }
}
