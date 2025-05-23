using System.Configuration;
using System.Text.RegularExpressions;
using Mysqlx.Resultset;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Functions.Reflection;

public abstract class Сommon(IDatabaseFixture db) : ExprTests(db)
{
     [Fact]
     public async Task FuncDbVersionTests()
     {
        var runner = await db.GetRunnerAsync();
        QueryBuilder qb = QueryBuilder.FromDual(new Factory().CreateExpressionResolver(db.GetDatabaseType()));
        qb = qb.Select(new()
        {
            ["test"] = "DbVersion()",
        })
        .Expect(e => e.JoinBy(", "));
        
        var result = await runner.RunQueryAsScalar(qb.Build());

        if (result is not TextValue(var text)) throw new Exception("Результат должен быть строкой");

        if (!Regex.IsMatch(text, @"^\d+(\.\d+)*$")) throw new Exception("Результат должен быть в формате версии");
     }
     
}