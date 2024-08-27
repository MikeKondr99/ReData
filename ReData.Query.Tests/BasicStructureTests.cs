using System.Text;
using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Tests;


internal class FakeExpressionBuilder : IExpressionBuilder
{
    public void Write(StringBuilder res, IExpr expr, IReadOnlyDictionary<string,ExprType> fields)
    {
        res.Append("EXPR");
    }
}

public class BasicStructureTests
{
    private static Query query = new Query("table",new Dictionary<string, ExprType>() {["column1"] = ExprType.String })
        {
            Select = 
            [
                Expr.Parse("column1").As("column1"),
                Expr.Parse("5").As("column1")
            ],
            Where =
            [
                new NullLiteral(),
            ],
            OrderBy = 
            [
                (new NullLiteral(),Query.Order.Asc),
                (new NullLiteral(),Query.Order.Desc)
            ],
            Limit = 100,
            Offset = 50,
        };
    
    
    [Fact]
    public void PostgresBasicQueryStructure()
    {
        var builder = new PostgresQueryBuilder() { ExpressionBuilder = new FakeExpressionBuilder() };
        var sql = builder.Build(query);

        sql.Should().Be("""
                        SELECT "column1", EXPR AS "column2" FROM "table"
                        WHERE EXPR
                        ORDER BY EXPR ASC, EXPR DESC
                        LIMIT 100
                        OFFSET 50
                        """);
    }

    [Fact]
    public void MsSqlBasicQueryStructure()
    {
        var builder = new MsSqlQueryBuilder()
        {
            ExpressionBuilder = new FakeExpressionBuilder()
        };
        var sql = builder.Build(query);

        sql.Should().Be("""
                        SELECT "column1", EXPR AS "column2" FROM "table"
                        WHERE EXPR
                        ORDER BY EXPR ASC, EXPR DESC
                        OFFSET 50 ROWS
                        FETCH NEXT 100 ROWS ONLY
                        """);
    }
}