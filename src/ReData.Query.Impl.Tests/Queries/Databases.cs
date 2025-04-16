using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Queries;

[Collection("Postgres")]
public class Postgres(PostgresDatabaseFixture db, PostgresAssets assets) : Сommon(db, assets);

[Collection("SqlServer")]
public class SqlServer(SqlServerDatabaseFixture db, SqlServerAssets assets) : Сommon(db, assets);

[Collection("MySql")]
public class MySql(MySqlDatabaseFixture db, MySqlAssets assets) : Сommon(db, assets);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseDatabaseFixture db, ClickHouseAssets assets) : Сommon(db, assets);

[Collection("Oracle")]
public class Oracle(OracleDatabaseFixture db, OracleAssets assets) : Сommon(db, assets);

