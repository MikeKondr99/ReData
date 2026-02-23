using System.Collections.Concurrent;
using ReData.Query.Core;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Executors;
using ReData.Query.Impl.Functions;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Impl.QueryCompilers;

namespace ReData.Query;

public static class Factory
{
    private static readonly ConcurrentDictionary<DatabaseType, FunctionStorage> FunctionStorages = new();

    public static IQueryExecutor CreateQueryExecuter(DatabaseType database)
    {
        return database switch
        {
            DatabaseType.PostgreSql => new PostgresExecutor()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.ClickHouse => new ClickHouseExecutor()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.SqlServer => new SqlServerExecutor()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.MySql => new MySqlExecutor()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.Oracle => new OracleExecutor()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
        };
    }

    public static ExpressionResolver CreateExpressionResolver(DatabaseType database)
    {
        return new ExpressionResolver()
        {
            LiteralResolver = CreateLiteralResolver(database),
        };
    }

    public static ILiteralResolver CreateLiteralResolver(DatabaseType database) => database switch
    {
        DatabaseType.PostgreSql => new PostgresLiteralResolver(),
        DatabaseType.SqlServer => new SqlServerLiteralResolver(),
        DatabaseType.MySql => new MySqlLiteralResolver(),
        DatabaseType.ClickHouse => new ClickHouseLiteralResolver(),
        DatabaseType.Oracle => new OracleLiteralResolver(),
        _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
    };

    public static IFunctionStorage CreateFunctionStorage(DatabaseType database)
    {
        return FunctionStorages.GetOrAdd(database, (key) =>
        {
            var newFs = database switch
            {
                DatabaseType.PostgreSql => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.PostgreSql),
                DatabaseType.SqlServer => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.SqlServer),
                DatabaseType.MySql => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.MySql),
                DatabaseType.ClickHouse => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.ClickHouse),
                DatabaseType.Oracle => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.Oracle),
            };
            return newFs;
        });
    }


    public static IQueryCompiler CreateQueryCompiler(DatabaseType database)
    {
        var expr = new ExpressionCompiler();
        var funcs = CreateFunctionStorage(database);

        return database switch
        {
            DatabaseType.PostgreSql => new PostgresQueryCompiler()
            {
                ExpressionCompiler = expr,
            },
            DatabaseType.MySql => new MySqlQueryCompiler()
            {
                ExpressionCompiler = expr,
            },
            DatabaseType.ClickHouse => new ClickHouseQueryCompiler()
            {
                ExpressionCompiler = expr,
            },
            DatabaseType.Oracle => new OracleQueryCompiler()
            {
                ExpressionCompiler = expr,
            },
            DatabaseType.SqlServer => new SqlServerQueryCompiler()
            {
                ExpressionCompiler = expr,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
        };
    }
}

public enum DatabaseType
{
    PostgreSql = 1,
    SqlServer = 2,
    MySql = 3,
    ClickHouse = 4,
    Oracle = 5,
}