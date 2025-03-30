using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.DateTime;

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture runner) : Сommon(runner);

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture runner) : Сommon(runner);

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture runner) : Сommon(runner);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture runner) : Сommon(runner);

[Collection("Oracle")]
public class Oracle(OracleDatabaseFixture runner) : Сommon(runner);
