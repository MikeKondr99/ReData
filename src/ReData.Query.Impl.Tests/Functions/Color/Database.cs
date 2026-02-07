using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Color;

#pragma warning disable SA1402

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture runner) : Common(runner);

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture runner) : Common(runner);

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture runner) : Common(runner);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture runner) : Common(runner);

// [Collection("Oracle")]
// public class Oracle(OracleDatabaseFixture runner) : Common(runner);

#pragma warning restore SA1402
