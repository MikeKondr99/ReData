using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Impl.Tests.Queries;

namespace ReData.Query.Impl.Tests.Functions.Aggregation;

#pragma warning disable CA1711
#pragma warning disable SA1402

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture db, PostgresAssets assets) : Сommon(db, assets);

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture db, SqlServerAssets assets) : Сommon(db, assets);

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture db, MySqlAssets assets) : Сommon(db, assets);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture db, ClickHouseAssets assets) : Сommon(db, assets);

// [Collection("Oracle")]
// public class Oracle(OracleDatabaseFixture db, OracleAssets assets) : Сommon(db, assets);

#pragma warning restore CA1711
#pragma warning restore SA1402
