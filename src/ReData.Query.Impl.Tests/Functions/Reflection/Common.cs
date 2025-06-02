using System.Configuration;
using System.Text.RegularExpressions;
using Mysqlx.Resultset;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Functions.Reflection;

public abstract partial class Сommon(IDatabaseFixture db) : ExprTests(db)
{
     [Fact]
     public async Task FuncDbVersionTests()
     {
        var runner = await db.GetRunnerAsync();
        QueryBuilder qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(db.GetDatabaseType()));
        qb = qb.Select(new()
        {
            ["test"] = "DbVersion()",
        })
        .Expect(e => e.JoinBy(", "));

        var result = await runner.RunQueryAsScalar(qb.Build());

        if (result is not TextValue(var text))
        {
            throw new Exception("Результат должен быть строкой");
        }

        if (!VersionRegex().IsMatch(text))
        {
            throw new Exception("Результат должен быть в формате версии");
        }
     }

     [GeneratedRegex(@"^\d+(\.\d+)*$")]
     private static partial Regex VersionRegex();
}