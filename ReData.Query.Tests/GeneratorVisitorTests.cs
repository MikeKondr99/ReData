using System.Runtime.InteropServices.JavaScript;
using System.Text;
using FluentAssertions;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Tests;

public class GeneratorVisitorTests
{
    
    [Fact]
    public void ShouldWorkWithFunctions()
    {
        FunctionStorage functions =
        [
            new () {
                Name = "num",
                Parameters = [ExprType.String],
                ReturnType = ExprType.OptionalNumber,
                Template = Template.Compile($"CAST({0} AS DECIMAL)")
            }
        ];
        
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>(),
            FunctionTypes = functions,
        };
        var visitor = new GeneratorVisitor()
        {
            StringBuilder = new StringBuilder(),
            FunctionTokens = functions,
            TypeVisitor = typeVisitor,
        };


        var res = visitor.Visit(Expr.Parse("num('1.32')")).ToString();
        
        res.Should().Be("CAST('1.32' AS DECIMAL)");
    }
    
    [Fact]
    public void ShouldWorkWithFunctionsRecursively()
    {
        FunctionStorage functions =
        [
            new () {
                Name = "num",
                Parameters = [ExprType.String],
                ReturnType = ExprType.OptionalNumber,
                Template = Template.Compile($"CAST({0} AS DECIMAL)")
            },
            new () {
                Name = "coalesce",
                Parameters = [ExprType.OptionalNumber,ExprType.Number],
                ReturnType = ExprType.Number,
                Template = Template.Compile($"COALESCE({0}, {1})"),
            },
            new () {
                Name = "+",
                Parameters = [ExprType.Number,ExprType.Number],
                ReturnType = ExprType.Number,
                Template = Template.Compile($"({0} + {1})")
            }
        ];
        
        var typeVisitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>(),
            FunctionTypes = functions,
        };
        var visitor = new GeneratorVisitor()
        {
            StringBuilder = new StringBuilder(),
            FunctionTokens = functions,
            TypeVisitor = typeVisitor,
        };
        
        var res = visitor.Visit(Expr.Parse("coalesce(num('1.2'), 0.0) + coalesce(num('s'), 0.0)")).ToString();

        res.Should().Be("(COALESCE(CAST('1.2' AS DECIMAL), 0.0) + COALESCE(CAST('s' AS DECIMAL), 0.0))");
    }
}