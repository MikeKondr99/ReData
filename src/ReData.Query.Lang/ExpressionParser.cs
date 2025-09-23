using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ReData.Query.Common;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang;

internal sealed partial class ExpressionParser : LangParserBaseVisitor<ExprNode>
{
    public override ExprNode VisitStart(LangParser.StartContext context)
    {
        return Visit(context.children[0]);
    }

    public override ExprNode VisitUnary(LangParser.UnaryContext context)
    {
        if (context.children is [TerminalNodeImpl op, LangParser.ExprContext expr])
        {
            return new FuncExprNode()
            { 
                Span = Span(context),
                Name = op.GetText(),
                Arguments = [VisitExpr(expr)],
                Kind = FuncExprKind.Unary,
            };
        }
        throw new Exception("Non valid unary expression");
    }

    public override ExprNode VisitScope(LangParser.ScopeContext context)
    {
        return Visit(context.children[1]);
    }

    public override ExprNode VisitBinary(LangParser.BinaryContext context)
    {
        
        if (context.children is [LangParser.ExprContext left, TerminalNodeImpl op, LangParser.ExprContext right])
        {
            return new FuncExprNode()
            {
                Span = Span(context),
                Name = op.GetText(),
                Arguments = [Visit(left), Visit(right)],
                Kind = FuncExprKind.Binary,
            };
        }
        throw new Exception("Non valid binary expression");
    }

    public override ExprNode VisitObjectFunction(LangParser.ObjectFunctionContext context)
    {
        var args = context.children.OfType<ParserRuleContext>().Select(a => Visit(a));
        return new FuncExprNode()
        {
            Span = Span(context),
            Name = context.children[2].GetText(),
            Arguments = args.ToArray(),
            Kind = FuncExprKind.Method,
        };
    }

    public override ExprNode VisitName(LangParser.NameContext context)
    {
        var name = context.GetText();
        var tokenLen = name.Length;
        if (name[0] is '[' && name[^1] is ']')
        {
            name = name[1..^1];
        }

        name = EscapeRegex().Replace(name, "]");

        return new NameExprNode(name)
        {
            Span = Span(context)
        };
    }


    public override ExprNode VisitString(LangParser.StringContext context)
    {
        List<ExprNode> exprs = new();
        foreach (var part in context.stringContents())
        {
            var expr = part.children.OfType<LangParser.ExprContext>().FirstOrDefault();

            void Append(ExprNode expr)
            {
                if (exprs.LastOrDefault() is StringLiteral last && expr is StringLiteral nw)
                {
                    exprs[^1] = new StringLiteral(last.Value + nw.Value, Span(part));
                } 
                else
                {
                    exprs.Add(expr);
                }
            }
            
            if (expr is not null)
            {
                var e = Visit(expr);
                Append(new FuncExprNode
                    {
                        Name = "Text",
                        Arguments = [e],
                        Kind = FuncExprKind.Default,
                        Span = e.Span,
                    });
            }
            else if(part.TEXT() is not null)
            {
                Append(new StringLiteral(part.GetText(), Span(part)));
            } 
            else if (part.ESCAPE_SEQUENCE() is not null)
            {
                 var chr = part.GetText()[1];
                 var value = chr switch
                 {
                     'r' => "\r",
                     'n' => "\n",
                     't' => "\t",
                     '\'' => "'",
                     '\\' => "\\",
                     _ => chr.ToString()
                 };
                 Append(new StringLiteral(value, Span(part)));
            }
            else
            {
                throw new Exception("else");
            }
        }

        if (exprs.Count is 0)
        {
            return new StringLiteral("");
        }

        var result = exprs.Aggregate((a, b) => new FuncExprNode
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments = [a, b],
            Span = Span(context),
        });
        return result;
    }

    public override ExprNode VisitInteger(LangParser.IntegerContext context)
    {
        return new IntegerLiteral(long.Parse(context.GetText()))
        {
            Span = Span(context),
        };
    }

    public override ExprNode VisitNumber(LangParser.NumberContext context)
    {
        return new NumberLiteral(double.Parse(context.GetText(), CultureInfo.InvariantCulture))
        {
            Span = Span(context),
        };
    }

    public override ExprNode VisitFunc(LangParser.FuncContext context)
    {
        var args = context.children.OfType<LangParser.ExprContext>().Select(a => Visit(a));
        return new FuncExprNode()
        {
            Span = Span(context),
            Name = context.children[0].GetText(),
            Arguments = args.ToArray(),
            Kind = FuncExprKind.Default,
        };
    }

    public override ExprNode VisitBoolean(LangParser.BooleanContext context)
    {
        return new BooleanLiteral(bool.Parse(context.GetText()))
        {
            Span = Span(context),
        };
    }


    public override ExprNode VisitNull(LangParser.NullContext context)
    {
        return new NullLiteral()
        {
            Span = Span(context)
        };
    }
    
    private static ExprSpan Span(ParserRuleContext context)
    {
        return new ExprSpan(
            (uint)context.Start.Line,
            (uint)context.Start.Column,
            (uint)context.Stop.Line,
            (uint)(context.Stop.Column + context.Stop.Text.Length)
        );
    }

    [GeneratedRegex(@"\\\]")]
    private static partial Regex EscapeRegex();
}
