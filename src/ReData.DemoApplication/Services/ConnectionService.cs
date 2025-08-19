using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Services;

public class ConnectionService
{
    public string Connection { get; init; }

    public ConnectionService(IConfiguration configuration)
    {
        Connection = configuration["DemoDbConnection"] ?? string.Empty;
    }

    public QueryBuilder GetQuery()
    {
        IReadOnlyList<(string name, FieldType type)> fields = [
            ("id",new FieldType(DataType.Integer, false)),
            ("customer_name",new FieldType(DataType.Text, true)),
            ("email",new FieldType(DataType.Text, true)),
            ("age",new FieldType(DataType.Integer, true)),
            ("account_balance",new FieldType(DataType.Number, true)),
            ("is_active",new FieldType(DataType.Bool, true)),
            ("signup_date",new FieldType(DataType.DateTime, true)),
            ("last_login",new FieldType(DataType.DateTime, true)),
            ("customer_category",new FieldType(DataType.Text, true)),
            ("random_number",new FieldType(DataType.Integer, true)),
            ("notes",new FieldType(DataType.Text, true)),
            ("purchase_count",new FieldType(DataType.Integer, true)),
        ];

        var query = QueryBuilder.FromTable(
            Factory.CreateExpressionResolver(DatabaseType.PostgreSql),
            ["test_data"],
            fields
        );
        return query;
    }


}