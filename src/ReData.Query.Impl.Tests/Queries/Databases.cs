using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Queries;

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture fixture) : Сommon(fixture);

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture fixture) : Сommon(fixture);

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture fixture) : Сommon(fixture);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture fixture) : Сommon(fixture);

[Collection("Oracle")]
public class Oracle(OracleDatabaseFixture fixture) : Сommon(fixture);

