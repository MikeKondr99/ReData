using ReData.DemoApp.Database;
using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApp.Services;

public class DwhService
{
    private readonly ApplicationDatabaseContext db;
    public string DwhReadConnection { get; init; }
    public string DwhWriteConnection { get; init; }

    public DwhService(IConfiguration configuration, ApplicationDatabaseContext db)
    {
        this.db = db;
        DwhReadConnection = configuration.GetConnectionString("DwhRead") ?? string.Empty;
        DwhWriteConnection = configuration.GetConnectionString("DwhWrite") ?? string.Empty;
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