using System.Data.Common;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public static class DbReaderExtensions
{
    public static async Task<IReadOnlyList<Dictionary<string, IValue>>> CollectToObjects(this Task<DbDataReader> readerTask, IEnumerable<Field> fields)
    {
        await using var reader = await readerTask;
        var fieldsArr = fields.ToArray();
        List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();

        while (await reader.ReadAsync())
        {
            var recordDict = new Dictionary<string, IValue>();

            for (int i = 0; i < fieldsArr.Length; i++)
            {
                var value = reader.GetValue(i);
                recordDict[fieldsArr[i].Alias] = DatabaseValuesMapper.MapField(value, fieldsArr[i].Type);
            }

            result.Add(recordDict);
        }

        return result;
    }
    
    public static async Task<IValue> CollectToScalar(this Task<DbDataReader> readerTask, IEnumerable<Field> fields)
    {
        await using var reader = await readerTask;
        if (await reader.ReadAsync())
        {
            var value = reader.GetValue(0);
            return DatabaseValuesMapper.MapField(value, fields.First().Type);
        }

        throw new Exception("Query не вернул значения хотя ожидался скаляр");
    }
}