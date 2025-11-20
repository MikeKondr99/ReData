using ReData.DemoApplication.Database;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Services;

public class ConnectionService
{
    private readonly ApplicationDatabaseContext db;
    public string Connection { get; init; }

    public ConnectionService(IConfiguration configuration, ApplicationDatabaseContext db)
    {
        this.db = db;
        Connection = configuration["DemoDbConnection"] ?? string.Empty;
    }

    public QueryBuilder GetQueryBuilder(Guid dataConnectorId)
    {
        var dataConnector = db.DataConnectors.FirstOrDefault(dc => dc.Id == dataConnectorId);

        if (dataConnector is null)
        {
            throw new Exception("Коннектор не найден");
        }

        var fieldList = dataConnector.FieldList;

        IReadOnlyList<(string name, FieldType type)> fields = fieldList
            .Select(field => (field.Alias, new FieldType(field.DataType, field.CanBeNull)))
            .ToList();

        return QueryBuilder.FromTable(
            Factory.CreateExpressionResolver(DatabaseType.PostgreSql),
            [dataConnector.TableName],
            fields
        );
    }
}