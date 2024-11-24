
using ReData.Query.Impl.Tests.Queries;
using ReData.Query.Impl.Tests.Runners;

namespace ReData.Query.Impl.Tests.Queries;

[Collection("Postgres")]
public class Postgres(PostgresRunner runner) : Сommon(runner);

[Collection("SqlServer")]
public class SqlServer(SqlServerRunner runner) : Сommon(runner);

[Collection("MySql")]
public class MySql(MySqlRunner runner) : Сommon(runner);

[Collection("ClickHouse")]
public class ClickHouse(ClickHouseRunner runner) : Сommon(runner);

[Collection("Oracle")]
public class Oracle(OracleRunner runner) : Сommon(runner);

