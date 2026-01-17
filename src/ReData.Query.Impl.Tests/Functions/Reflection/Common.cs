using System.Configuration;
using System.Text.RegularExpressions;
using Mysqlx.Resultset;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Core.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Functions.Reflection;

public abstract partial class Сommon(IDatabaseFixture db) : ExprTests(db)
{
     [Fact]
     public async Task FuncDbVersionTests()
     {
        var runner = await db.GetRunnerAsync();
        QueryBuilder qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(db.GetDatabaseType()), Factory.CreateFunctionStorage(db.GetDatabaseType()));
        qb = qb.Select(new()
        {
            ["test"] = "DbVersion()",
        })
        .Expect(e => e.JoinBy(", "));

        var result = await GetScalarAsync(qb);

        if (result is not TextValue(var text))
        {
            throw new Exception("Результат должен быть строкой");
        }

        if (!VersionRegex().IsMatch(text))
        {
            throw new Exception("Результат должен быть в формате версии");
        }
     }


     [Theory(DisplayName = "Type")]
     // Int types
     [InlineData("Type(42)", "int!")]
     [InlineData("Type(If(false, 42, null))", "int")]
     [InlineData("Type(Int(null))", "int")]

     // Text types  
     [InlineData("Type('hello')", "text!")]
     [InlineData("Type(If(false, 'hello', null))", "text")]
     [InlineData("Type(''.EmptyIsNull())", "text")]

     // Num types
     [InlineData("Type(3.14)", "num!")]
     [InlineData("Type(If(false, 3.14, null))", "num")]
     [InlineData("Type(Num(null))", "num")]

     // Date types
     [InlineData("Type(Date(2023,1,1))", "date!")]
     [InlineData("Type(Date(null,1,1))", "date")]

     // Bool types
     [InlineData("Type(true)", "bool!")]
     [InlineData("Type(Bool(null))", "bool")]
     
     // Null
     [InlineData("Type(null)", "null")]
     public Task FuncTypeTests(string expr, object? expected) => Test(expr, expected);

     [GeneratedRegex(@"^\d+(\.\d+)*$")]
     private static partial Regex VersionRegex();
}