using ReData.Query.Impl.Tests.Queries;
using ReData.Query.Impl.Tests.Runners;

namespace ReData.Query.Impl.Tests;

[CollectionDefinition("Postgres")]
public class PostgreRunnerCollection : ICollectionFixture<PostgresRunner>;

[CollectionDefinition("MySql")]
public class MySqlRunnerCollection : ICollectionFixture<MySqlRunner>;

[CollectionDefinition("SqlServer")]
public class SqlServerRunnerCollection : ICollectionFixture<SqlServerRunner>;

[CollectionDefinition("ClickHouse")]
public class ClickHouseRunnerCollection : ICollectionFixture<ClickHouseRunner>;

[CollectionDefinition("Oracle")]
public class OracleRunnerCollection : ICollectionFixture<OracleRunner>;
