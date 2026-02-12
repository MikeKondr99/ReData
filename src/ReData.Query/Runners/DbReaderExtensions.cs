using System.Data.Common;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public static class DbReaderExtensions
{
    public static async Task<IReadOnlyList<Dictionary<string, IValue>>> CollectToObjects(this Task<DbDataReader> readerTask, IEnumerable<Field> fields)
    {
        var fieldsArr = fields.ToArray();
        await using var reader = await readerTask;
        await using var mappedReader = new LegacyIValueDbDataReader(reader, fieldsArr);
        List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();

        while (await mappedReader.ReadAsync())
        {
            var recordDict = new Dictionary<string, IValue>();

            for (int i = 0; i < fieldsArr.Length; i++)
            {
                var value = mappedReader.GetValue(i);
                recordDict[fieldsArr[i].Alias] = (IValue)value;
            }

            result.Add(recordDict);
        }

        return result;
    }
    
    public static async Task<IValue> CollectToScalar(this Task<DbDataReader> readerTask, IEnumerable<Field> fields)
    {
        var fieldsArr = fields.ToArray();
        await using var reader = await readerTask;
        await using var mappedReader = new LegacyIValueDbDataReader(reader, fieldsArr);
        if (await mappedReader.ReadAsync())
        {
            var value = mappedReader.GetValue(0);
            return (IValue)value;
        }

        throw new Exception("Query не вернул значения хотя ожидался скаляр");
    }
}
