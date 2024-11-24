using FluentAssertions;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Tests;

public class TypeVisitorTests
{
    // [Theory]
    // [InlineData("2",DataType.Integer)]
    // [InlineData("2.23",DataType.Number)]
    // [InlineData("null",DataType.Null)]
    // [InlineData("'some string '",DataType.Text)]
    // [InlineData("true",DataType.Boolean)]
    // public void ShouldWorkWithLiterals(string expr, DataType expected)
    // {
    //     var visitor = new TypeVisitor()
    //     {
    //         FieldTypes = new EmptyFieldStorage(),
    //         FunctionTypes = new FunctionStorage(),
    //     };
    //
    //     var type = visitor.Visit(Expr.Parse(expr));
    //
    //     type.Should().Be(expected);
    // }
    
    // [Theory]
    // [InlineData(DataType.Number)]
    // [InlineData(DataType.Integer)]
    // [InlineData(DataType.Boolean)]
    // [InlineData(DataType.Null)]
    // [InlineData(DataType.Text)]
    // public void ShouldWorkWithFields(DataType expected)
    // {
    //     var visitor = new TypeVisitor()
    //     {
    //         FieldTypes = new FieldStorage([new Query.Field("field",ExprType.FromDataType(expected))]),
    //         FunctionTypes = new FunctionStorage(),
    //     };
    //
    //     var type = visitor.Visit(Expr.Parse("field"));
    //
    //     type.Should().Be(expected);
    // }
    
    // [Fact]
    // public void ShouldWorkWithFunctions()
    // {
    //     FunctionStorage functions =
    //     [
    //         new()
    //         {
    //             Name = "num",
    //             Arguments = [ExprType.Text()],
    //             ReturnType = ExprType.Number().Optional(),
    //             Template = Template.Compile($"")
    //         },
    //     ];
    //     var visitor = new TypeVisitor()
    //     {
    //         FieldTypes = new EmptyFieldStorage(),
    //         FunctionTypes = functions
    //     };
    //
    //     var type = visitor.Visit(Expr.Parse("num('1.32')"));
    //
    //     type.Should().Be(ExprType.Number().Optional());
    // }
    //
    // [Fact]
    // public void ShouldWorkWithFunctionsRecursively()
    // {
    //     FunctionStorage functions =
    //     [
    //         new () {
    //             Name = "num",
    //             Arguments = [ExprType.Text()],
    //             ReturnType = ExprType.Number().Optional(),
    //             Template = Template.Compile($"CAST({0} AS DECIMAL)")
    //         },
    //         new () {
    //             Name = "coalesce",
    //             Arguments = [ExprType.Number().Optional(),ExprType.Number()],
    //             ReturnType = ExprType.Number(),
    //             Template = Template.Compile($"COALESCE({0}, {1})"),
    //         },
    //         new () {
    //             Name = "+",
    //             Arguments = [ExprType.Number(),ExprType.Number()],
    //             ReturnType = ExprType.Number(),
    //             Template = Template.Compile($"({0} + {1})")
    //         }
    //     ];
    //     var visitor = new TypeVisitor()
    //     {
    //         FieldTypes = new EmptyFieldStorage(),
    //         FunctionTypes = functions
    //     };
    //
    //     var type = visitor.Visit(Expr.Parse("coalesce(num('1.2'), 0.0) + coalesce(num('s'), 0.0)"));
    //
    //     type.Should().Be(DataType.Number);
    // }
    
    
    
    
    
    
    
    
    
}