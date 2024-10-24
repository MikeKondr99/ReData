using ReData.Query.Impl.Functions;
using ReData.Query.Impl.LiteralBuilders;

namespace ReData.Query.Impl.QueryBuilders;

public class QueryBuilderFactory
{
    private static readonly IReadOnlyDictionary<DatabaseType, FunctionStorage> _functionStorages =
        new Dictionary<DatabaseType, FunctionStorage>()
        {
            [DatabaseType.PostgreSql] = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.PostgreSql),
            [DatabaseType.SqlServer] = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.SqlServer),
            [DatabaseType.MySql] = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.MySql),
            [DatabaseType.ClickHouse] = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.ClickHouse),
            [DatabaseType.Oracle] = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.Oracle),
        };
    
    
    private static readonly IReadOnlyDictionary<DatabaseType, IQueryBuilder> _builders 
        = new Dictionary<DatabaseType, IQueryBuilder>()
    {
        [DatabaseType.PostgreSql] = new PostgresQueryBuilder()
        {
            FunctionsStorage = _functionStorages[DatabaseType.PostgreSql],
            ExpressionBuilder = new ExpressionBuilder()
            {
                FunctionStorage = _functionStorages[DatabaseType.PostgreSql],
                LiteralBuilder = new PostgresLiteralBuilder()
            }
        },
        [DatabaseType.SqlServer] = new SqlServerQueryBuilder()
        {
            FunctionsStorage = _functionStorages[DatabaseType.SqlServer],
            ExpressionBuilder = new ExpressionBuilder()
            {
                FunctionStorage = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.SqlServer),
                LiteralBuilder = new SqlServerLiteralBuilder()
            }
        },
        [DatabaseType.MySql] = new MySqlQueryBuilder()
        {
            FunctionsStorage = _functionStorages[DatabaseType.MySql],
            ExpressionBuilder = new ExpressionBuilder()
            {
                FunctionStorage = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.MySql),
                LiteralBuilder = new MySqlLiteralBuilder()
            }
        },
        [DatabaseType.ClickHouse] = new ClickHouseQueryBuilder()
        {
            FunctionsStorage = _functionStorages[DatabaseType.ClickHouse],
            ExpressionBuilder = new ExpressionBuilder()
            {
                FunctionStorage = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.ClickHouse),
                LiteralBuilder = new ClickHouseLiteralBuilder()
            }
        },
        [DatabaseType.Oracle] = new OracleQueryBuilder()
        {
            FunctionsStorage = _functionStorages[DatabaseType.Oracle],
            ExpressionBuilder = new ExpressionBuilder()
            {
                FunctionStorage = GlobalFunctionsStorage.GetFunctions(DatabaseTypeFlags.Oracle), 
                LiteralBuilder = new OracleLiteralBuilder()
            }
        },
    };
    
    
    public IQueryBuilder Create(DatabaseType type)
    {
        return _builders[type];
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