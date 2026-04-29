using ReData.DemoApp.Database;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApp.Services;

public class ConnectorQueryBuilderService

{
    private readonly ApplicationDatabaseContext db;

    public ConnectorQueryBuilderService(ApplicationDatabaseContext db)
    {
        this.db = db;
    }

    public QueryBuilder GetQueryBuilder(Guid dataConnectorId, IConstantRuntime? constantRuntime = null)
    {
        var dataConnector = db.DataConnectors.Find(dataConnectorId);

        if (dataConnector is null)
        {
            throw new Exception("Коннектор не найден");
        }

        var fieldList = dataConnector.FieldList;

        IReadOnlyList<(string name, string column, FieldType type)> fields = fieldList
            .Select(field => (field.Alias, field.Column, new FieldType(field.DataType, field.CanBeNull)))
            .ToList();

        return QueryBuilder.FromTable(
            Factory.CreateExpressionResolver(DatabaseType.PostgreSql),
            Factory.CreateFunctionStorage(DatabaseType.PostgreSql),
            [dataConnector.TableName],
            fields,
            constantRuntime
        );
    }
}
