namespace ReData.Query;

public class QueryBuilderFactory
{
    
    
    public IQueryBuilder Create(DatabaseType type)
    {
        return new PostgresQueryBuilder()
        {
            ExpressionBuilder = new ExpressionBuilder() { FunctionStorage = new FunctionStorage() }
        };

    }
    
}

public enum DatabaseType
{
    PostgreSql = 1,
    MsSql = 2,
}