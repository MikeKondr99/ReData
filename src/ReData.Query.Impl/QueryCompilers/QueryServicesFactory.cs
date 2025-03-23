using ClickHouse.Client.ADO;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Impl.Functions;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.QueryBuilders;

public class QueryServicesFactory
{
    private static readonly IDictionary<DatabaseType, FunctionStorage> _functionStorages =
        new Dictionary<DatabaseType, FunctionStorage>();

    public IQueryRunner CreateQueryRunner(DatabaseType database, string connectionString)
    {
        return database switch
        {
            DatabaseType.PostgreSql => new PostgresRunner()
            {
                Connection = new NpgsqlConnection(connectionString),
                Mapper = new DatabaseValuesMapper(),
                FunctionStorage = CreateFunctionStorage(database),
                QueryCompiler = CreateQueryCompiler(database),

            },
            DatabaseType.ClickHouse => new ClickHouseRunner()
            {
                Connection = new ClickHouseConnection(connectionString),
                Mapper = new DatabaseValuesMapper(),
                FunctionStorage = CreateFunctionStorage(database),
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.SqlServer => new SqlServerRunner()
            {
                Connection = new SqlConnection(connectionString),
                Mapper = new DatabaseValuesMapper(),
                FunctionStorage = CreateFunctionStorage(database),
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.MySql => new MySqlRunner()
            {
                Connection = new MySqlConnection(connectionString),
                Mapper = new DatabaseValuesMapper(),
                FunctionStorage = CreateFunctionStorage(database),
                QueryCompiler = CreateQueryCompiler(database),
            },
            DatabaseType.Oracle => new OracleRunner()
            {
                Connection = new OracleConnection(connectionString),
                Mapper = new DatabaseValuesMapper(),
                FunctionStorage = CreateFunctionStorage(database),
                QueryCompiler = CreateQueryCompiler(database),
            },
            
        };
    }
    
    public IExpressionBuilder CreateExpressionBuilder(DatabaseType database) => new ExpressionBuilder()
    {
        FunctionStorage = CreateFunctionStorage(database),
        LiteralBuilder = CreateLiteralBuilder(database),
    };

    public ILiteralBuilder CreateLiteralBuilder(DatabaseType database) => database switch
    {
        DatabaseType.PostgreSql => new PostgresLiteralBuilder(),
        DatabaseType.SqlServer => new SqlServerLiteralBuilder(),
        DatabaseType.MySql => new MySqlLiteralBuilder(),
        DatabaseType.ClickHouse => new ClickHouseLiteralBuilder(),
        DatabaseType.Oracle => new OracleLiteralBuilder(),
        _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
    };

    public IFunctionStorage CreateFunctionStorage(DatabaseType database)
    {
        if (_functionStorages.TryGetValue(database, out var fs))
        {
            return fs;
        }

        var newFs = database switch
        {
            DatabaseType.PostgreSql => GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.PostgreSql),
            DatabaseType.SqlServer => GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.SqlServer),
            DatabaseType.MySql => GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.MySql),
            DatabaseType.ClickHouse => GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.ClickHouse),
            DatabaseType.Oracle => GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.Oracle),
        };
        _functionStorages[database] = newFs;
        return newFs;

    }


    public IQueryCompiler CreateQueryCompiler(DatabaseType database)
    {
        var expr = CreateExpressionBuilder(database);
        var funcs = CreateFunctionStorage(database);

        return database switch
        {
            DatabaseType.PostgreSql => new PostgresQueryCompiler()
            {
                ExpressionBuilder = expr,
                FunctionsStorage = funcs
            },
            DatabaseType.MySql => new MySqlQueryCompiler()
            {
                ExpressionBuilder = expr,
                FunctionsStorage = funcs
            },
            DatabaseType.ClickHouse => new ClickHouseQueryCompiler()
            {
                ExpressionBuilder = expr,
                FunctionsStorage = funcs
            },
            DatabaseType.Oracle => new OracleQueryCompiler()
            {
                ExpressionBuilder = expr,
                FunctionsStorage = funcs
            },
            DatabaseType.SqlServer => new SqlServerQueryCompiler()
            {
                ExpressionBuilder = expr,
                FunctionsStorage = funcs
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