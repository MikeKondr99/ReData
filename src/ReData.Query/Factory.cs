using System.Collections.Concurrent;
using ClickHouse.Client.ADO;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Core;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Impl.Functions;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Impl.QueryCompilers;
using ReData.Query.Runners;

namespace ReData.Query;

public static class Factory
{
    private static readonly ConcurrentDictionary<DatabaseType, FunctionStorage> FunctionStorages = new();

    public static IQueryRunner CreateQueryRunner(DatabaseType database)
    {
        return database switch
        {
            DatabaseType.PostgreSql => new PostgresRunner()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.ClickHouse => new ClickHouseRunner()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.SqlServer => new SqlServerRunner()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.MySql => new MySqlRunner()
            {
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.Oracle => new OracleRunner()
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