using FluentResults;
using Npgsql;

namespace ReData.Domain.Services.QueryBuilder;

public interface IDataSourceConnector
{
    public Result CheckConnection(Domain.DataSource dataSource);
    
    public Entity.DataSourceType Type { get; }

}
public interface IDataSourceConnector<T>
{
    public Result<T> GetConnector(Domain.DataSource dataSource);

    public Result CheckConnection(Domain.DataSource dataSource) => GetConnector(dataSource).ToResult();
}

public interface IDataSourceConnectorFactory
{
    public IDataSourceConnector GetChecker(Entity.DataSourceType type);
}

public static class DataSourceConnectionCheckerFactoryExtension
{
    public static Result CheckConnection(this IDataSourceConnectorFactory factory, Domain.DataSource dataSource) =>
        factory.GetChecker(dataSource.Type).CheckConnection(dataSource);

}

public sealed class DataSourceConnectionCheckFactory : IDataSourceConnectorFactory
{
    public required IEnumerable<IDataSourceConnector> ConnectionCheckers { private get; init; }
    
    private Dictionary<Entity.DataSourceType, IDataSourceConnector>? _connectionCheckers;

    private Dictionary<Entity.DataSourceType, IDataSourceConnector> Checkers =>
        _connectionCheckers ??= ConnectionCheckers.ToDictionary(c => c.Type, c => c);
    
    public IDataSourceConnector GetChecker(Entity.DataSourceType type)
    {
        return Checkers[type];
    }
}

public sealed class PostgresConnector : IDataSourceConnector
{
    public Entity.DataSourceType Type => Entity.DataSourceType.PostgreSql;
    
    public Result CheckConnection(Domain.DataSource dataSource)
    {
        try
        {
            var connection =
                new NpgsqlConnection(String.Join(";", dataSource.Parameters.Select(p => $"{p.Key}={p.Value}")));
            connection.Open();
            connection.Close();
            return Result.Ok();
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(new ConnectionInvalid(new ()
            {
                [ex.ParamName ?? ""] = "Unknown parameter",
            }));
        }
        catch (NpgsqlException ex)
        {
            if (ex.Message.StartsWith("28P01"))
            {
                return Result.Fail(new ConnectionAuthFailed());
            }
            
            if (ex.Message.StartsWith("No password"))
            {
                return Result.Fail(new ConnectionInvalid(new ()
                {
                    ["Password"]= "IsRequired"
                }));
            }
            return Result.Fail(new ConnectionCrushed(ex.Message));
        }
    }
}

public sealed class CsvConnector : IDataSourceConnector
{
    public Entity.DataSourceType Type => Entity.DataSourceType.Csv;
    
    public Result CheckConnection(Domain.DataSource dataSource) => Result.Ok();
}