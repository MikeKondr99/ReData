using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication;

public class ConnectionService
{
    public string Connection { get; init; }

    public ConnectionService(IConfiguration configuration)
    {
        Connection = configuration["CONNECTION_STRING"] ?? "";
    }

    public QueryBuilder GetQuery()
    {
        var factory = new Factory();
        var query = QueryBuilder.FromTable(
            factory.CreateExpressionResolver(DatabaseType.PostgreSql),
            ["test_data"],
            [
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
            ]
        );
        return query;
    }
    

}