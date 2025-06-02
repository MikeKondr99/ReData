using ReData.Query.Impl.Tests.Queries;

namespace ReData.Query.Impl.Tests.Fixtures;

#pragma warning disable CA1711
#pragma warning disable SA1402

[CollectionDefinition("Postgres")]
public class PostgreRunnerCollection : ICollectionFixture<PostgresDatabaseFixture>, ICollectionFixture<PostgresAssets>;

[CollectionDefinition("MySql")]
public class MySqlRunnerCollection : ICollectionFixture<MySqlDatabaseFixture>, ICollectionFixture<MySqlAssets>;

[CollectionDefinition("SqlServer")]
public class SqlServerRunnerCollection : ICollectionFixture<SqlServerDatabaseFixture>,
    ICollectionFixture<SqlServerAssets>;

[CollectionDefinition("ClickHouse")]
public class ClickHouseRunnerCollection : ICollectionFixture<ClickHouseDatabaseFixture>,
    ICollectionFixture<ClickHouseAssets>;

[CollectionDefinition("Oracle")]
public class OracleRunnerCollection : ICollectionFixture<OracleDatabaseFixture>, ICollectionFixture<OracleAssets>;

#pragma warning restore CA1711 SA1401
#pragma warning restore SA1402
