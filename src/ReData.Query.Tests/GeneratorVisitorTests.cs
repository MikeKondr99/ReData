using System.Runtime.InteropServices.JavaScript;
using System.Text;
using FluentAssertions;
using ReData.Query.Impl.LIterals;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Tests;

public class GeneratorVisitorTests
{
    //
    // [Fact]
    // public void ShouldWorkWithFunctions()
    // {
    //     FunctionStorage functions =
    //     [
    //         new () {
    //             Name = "num",
    //             Arguments = [ExprType.Text()],
    //             ReturnType = ExprType.Number().Optional(),
    //             Template = Template.Compile($"CAST({0} AS DECIMAL)")
    //         }
    //     ];
    //     
    //     var typeVisitor = new TypeVisitor()
    //     {
    //         FieldTypes = new EmptyFieldStorage(),
    //         FunctionTypes = functions,
    //     };
    //     var visitor = new GeneratorVisitor()
    //     {
    //         StringBuilder = new StringBuilder(),
    //         FunctionStorage = functions,
    //         LiteralBuilder = new TestLiteralBuilder(),
    //         TypeVisitor = typeVisitor,
    //     };
    //
    //
    //     var res = visitor.Visit(Expr.Parse("num('1.32')")).ToString();
    //     
    //     res.Should().Be("CAST('1.32' AS DECIMAL)");
    // }
    
    // [Fact]
    // public void ShouldWorkWithFunctionsRecursively()
    // {
    //     FunctionStorage functions =
    //     [
    //     //     new () {
    //     //         Name = "num",
    //     //         Arguments = [ExprType.Text()],
    //     //         ReturnType = ExprType.Number().Optional(),
    //     //         Template = Template.Compile($"CAST({0} AS DECIMAL)")
    //     //     },
    //     //     new () {
    //     //         Name = "coalesce",
    //     //         Arguments = [ExprType.Number().Optional(),ExprType.Number()],
    //     //         ReturnType = ExprType.Number(),
    //     //         Template = Template.Compile($"COALESCE({0}, {1})"),
    //     //     },
    //     //     new () {
    //     //         Name = "+",
    //     //         Arguments = [ExprType.Number(),ExprType.Number()],
    //     //         ReturnType = ExprType.Number(),
    //     //         Template = Template.Compile($"({0} + {1})")
    //     //     }
    //     // ];
    //     
    //     // var typeVisitor = new TypeVisitor()
    //     // {
    //     //     FieldTypes = new EmptyFieldStorage(),
    //     //     FunctionTypes = functions,
    //     // };
    //     // var visitor = new GeneratorVisitor()
    //     // {
    //     //     StringBuilder = new StringBuilder(),
    //     //     FunctionStorage = functions,
    //     //     TypeVisitor = typeVisitor,
    //     //     LiteralBuilder = new TestLiteralBuilder()
    //     // };
    //     
    //     // var res = visitor.Visit(Expr.Parse("coalesce(num('1.2'), 0.) + coalesce(num('s'), 0.)")).ToString();
    //     //
    //     // res.Should().Be("(COALESCE(CAST('1.2' AS DECIMAL), 0) + COALESCE(CAST('s' AS DECIMAL), 0))");
    // }
}