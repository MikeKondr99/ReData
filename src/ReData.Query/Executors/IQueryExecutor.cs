using System.Data.Common;

namespace ReData.Query.Executors;

public interface IQueryExecutor
{
    // Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query);

    Task<DomainDbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection);

    // async Task<IReadOnlyList<Dictionary<string, IValue>>> RunQueryAsObjectAsync(Core.Query query,
    //     DbConnection connection)
    // {
    //     var dbReader = await GetDataReaderAsync(query, connection);
    //
    //     var fields = query.Fields().ToArray();
    //
    //     List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();
    //
    //     while (await dbReader.ReadAsync())
    //     {
    //         var recordDict = new Dictionary<string, IValue>();
    //
    //         for (int i = 0; i < fields.Length; i++)
    //         {
    //             var value = dbReader.GetValue(i);
    //         }
    //
    //         result.Add(recordDict);
    //     }
    //
    //     return result;
    // }
    //
    // public async Task<IValue> RunQueryAsScalarAsync(Core.Query query, DbConnection connection)
    // {
    //     var dbReader = await GetDataReaderAsync(query, connection);
    //     if (await dbReader.ReadAsync())
    //     {
    //         var value = dbReader.GetValue(0);
    //     }
    //
    //     throw new Exception("Query не вернул значения хотя ожидался скаляр");
    // }
}
