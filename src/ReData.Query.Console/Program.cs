using System.Net.Mime;
using ReData.Query;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Visitors;
using DateTime = System.DateTime;

var factory = new QueryServicesFactory();

var connection = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=pix_bi_test;Pooling=true;";
var runner = factory.CreateQueryRunner(DatabaseType.PostgreSql, connection);

var applications = QueryBuilder.FromTable(
    factory.CreateExpressionResolver(DatabaseType.PostgreSql),
    ["Applications"],
    [
        ("Id",new FieldType(DataType.Integer, true)),
        ("Name",new FieldType(DataType.Text, true)),
        ("Description",new FieldType(DataType.Text, true)),
        ("DirectoryId",new FieldType(DataType.Integer, true)),
        ("Icon",new FieldType(DataType.Unknown, true)),
    ]
);

while (true)
{
    try
    {
        Console.Write("WHERE: ");
        var where = Console.ReadLine() ?? "";
        var qb = applications.Select(new()
        {
            ["Id"] = "Id",
            ["Name"] = "Name",
            ["Description"] = "EmptyIsNull(Description)",
            ["DirectoryId"] = "DirectoryId.Text()",
            ["Icon"] = "Icon.Text().Substring(0,25)"
        }).Where(where);
        var data = await runner.RunQueryAsObjectAsync(qb.Build());
        foreach (var rec in data)
        {
            rec.PrintFancyInline();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}



public static class DictionaryPrinter
{
    public static void PrintFancyInline<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        string formatted = string.Join(", ", 
            dictionary.Select(kvp => 
                $"{FormatKey(kvp.Key)}: {kvp.Value}"));

        Console.WriteLine($"{{ {formatted} }}");
    }

    private static string FormatKey<TKey>(TKey key)
    {
        return $"{key?.ToString() ?? "null"}";
    }

}



