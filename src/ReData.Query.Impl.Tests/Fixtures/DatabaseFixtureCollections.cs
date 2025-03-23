namespace ReData.Query.Impl.Tests.Fixtures;

[CollectionDefinition("Postgres")]
public class PostgreRunnerCollection : ICollectionFixture<PostgresDatabaseFixture>;

[CollectionDefinition("MySql")]
public class MySqlRunnerCollection : ICollectionFixture<MySqlDatabaseFixture>;

[CollectionDefinition("SqlServer")]
public class SqlServerRunnerCollection : ICollectionFixture<SqlServerDatabaseFixture>;

[CollectionDefinition("ClickHouse")]
public class ClickHouseRunnerCollection : ICollectionFixture<ClickHouseDatabaseFixture>;

[CollectionDefinition("Oracle")]
public class OracleRunnerCollection : ICollectionFixture<OracleDatabaseFixture>;
