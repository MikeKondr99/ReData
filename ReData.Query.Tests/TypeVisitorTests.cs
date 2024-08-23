using FluentAssertions;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Tests;

public class TypeVisitorTests
{
    [Theory]
    [InlineData("2",ExprType.Integer)]
    [InlineData("2.23",ExprType.Number)]
    [InlineData("null",ExprType.Null)]
    [InlineData("'some string '",ExprType.String)]
    [InlineData("true",ExprType.Boolean)]
    public void ShouldWorkWithLiterals(string expr, ExprType expected)
    {
        var visitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>(),
            FunctionTypes = new FunctionStorage(),
        };

        var type = visitor.Visit(Expr.Parse(expr));

        type.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(ExprType.Number)]
    [InlineData(ExprType.Integer)]
    [InlineData(ExprType.Boolean)]
    [InlineData(ExprType.Null)]
    [InlineData(ExprType.String)]
    public void ShouldWorkWithFields(ExprType expected)
    {
        var visitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>()
            {
                ["field"] = expected,
            },
            FunctionTypes = new FunctionStorage(),
        };

        var type = visitor.Visit(Expr.Parse("field"));

        type.Should().Be(expected);
    }
    
    [Fact]
    public void ShouldWorkWithFunctions()
    {
        FunctionStorage functions =
        [
            new()
            {
                Name = "num",
                Parameters = [ExprType.String],
                ReturnType = ExprType.OptionalNumber,
                Template = Template.Compile($"")
            },
        ];
        var visitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>(),
            FunctionTypes = functions
        };

        var type = visitor.Visit(Expr.Parse("num('1.32')"));

        type.Should().Be(ExprType.OptionalNumber);
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
        var visitor = new TypeVisitor()
        {
            FieldTypes = new Dictionary<string, ExprType>(),
            FunctionTypes = functions
        };

        var type = visitor.Visit(Expr.Parse("coalesce(num('1.2'), 0.0) + coalesce(num('s'), 0.0)"));

        type.Should().Be(ExprType.Number);
    }
    
    
    
    
    
    
    
    
    
}