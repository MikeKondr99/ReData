using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Reflection;

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture runner) : Сommon(runner)
{
     [Fact]
     public Task FuncDbVersionTests() => Test("DbVersion()", "15.1");
}

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture runner) : Сommon(runner)
{
     [Fact]
     public Task FuncDbVersionTests() => Test("DbVersion()", "16.0.4135.4");
}

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture runner) : Сommon(runner)
{
     [Fact]
     public Task FuncDbVersionTests() => Test("DbVersion()", "8.0.41");
}

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture runner) : Сommon(runner)
{
     [Fact]
     public Task FuncDbVersionTests() => Test("DbVersion()", "23.6.3.87");
}

[Collection("Oracle")]
public class Oracle(OracleDatabaseFixture runner) : Сommon(runner)
{
     [Fact]
     public Task FuncDbVersionTests() => Test("DbVersion()", "21.0.0.0.0");
}
