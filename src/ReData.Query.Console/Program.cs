

using Npgsql;
using ReData.Query;
using ReData.Query.Impl.Functions;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Lang.Expressions;

var connection = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=pix_bi_test;Pooling=true;";
var factory = new QueryServicesFactory();
var runner = factory.CreateQueryRunner(DatabaseType.PostgreSql, connection);

var applications = new Query()
{
    From = new Table("Applications", [
        new Query.Field("Id", ExprType.Integer()),
        new Query.Field("Name", ExprType.Text()),
        new Query.Field("Description", ExprType.Text()),
        new Query.Field("Icon", ExprType.Unknown()),
        new Query.Field("DirectoryId", ExprType.Integer()),
    ]),
};

while (true)
{
    try
    {
        Console.Write("WHERE: ");
        var input = Console.ReadLine();
        var query = applications
            .Select(new Dictionary<string, string>()
            {
                ["Field"] = input,
            })
            .Take(20);
        var data = await runner.RunQueryAsync(query);
        foreach (var rec in data)
        {
            Console.WriteLine(rec);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}



