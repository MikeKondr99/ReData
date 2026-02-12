using System.Data.Common;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public static class DbReaderExtensions
{
public static DbDataReader MapFields(this DbDataReader reader, IEnumerable<Field> fields)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(fields);
        return new FieldAliasDbDataReader(reader, fields);
    }

    public static DbDataReader ClrNormalize(this DbDataReader reader, IEnumerable<Field> fields)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(fields);
        return new ClrMappedDbDataReader(reader, fields);
    }

    [Obsolete("Use ClrNormalize")]
    public static DbDataReader ClrNormilize(this DbDataReader reader, IEnumerable<Field> fields) => ClrNormalize(reader, fields);

    public static DbDataReader WrapInValue(this DbDataReader reader, IEnumerable<Field> fields)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(fields);
        return new LegacyIValueDbDataReader(reader, fields);
    }

    public static async Task<IReadOnlyList<Dictionary<string, IValue>>> CollectToObjects(this Task<DbDataReader> readerTask, IEnumerable<Field> fields)
    {
        var fieldsArr = fields.ToArray();
        await using var mappedReader = (await readerTask).WrapInValue(fieldsArr);
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
        await using var mappedReader = (await readerTask).WrapInValue(fieldsArr);
        if (await mappedReader.ReadAsync())
        {
            var value = mappedReader.GetValue(0);
            return (IValue)value;
        }

        throw new Exception("Query не вернул значения хотя ожидался скаляр");
    }
}
