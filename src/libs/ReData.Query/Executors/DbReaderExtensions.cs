using System.Data.Common;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;

namespace ReData.Query.Executors;

public static class DbReaderExtensions
{

    public static DomainDbDataReader ToDomain(this DbDataReader reader, IEnumerable<Field> fields)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(fields);
        return new DomainDbDataReader(reader, fields);
    }

    public static async Task<IReadOnlyList<Dictionary<string, IValue>>> CollectToObjects(
        this Task<DomainDbDataReader> readerTask)
    {
        await using var reader = await readerTask;
        await using var valueReader = new LegacyIValueDbDataReader(reader);
        List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();

        while (await valueReader.ReadAsync())
        {
            var record = new Dictionary<string, IValue>();
            for (var i = 0; i < valueReader.FieldCount; i++)
            {
                record[valueReader.GetName(i)] = (IValue)valueReader.GetValue(i);
            }

            result.Add(record);
        }

        return result;
    }

    public static async Task<IValue> CollectToScalar(this Task<DomainDbDataReader> readerTask)
    {
        await using var reader = await readerTask;
        await using var valueReader = new LegacyIValueDbDataReader(reader);

        if (await valueReader.ReadAsync())
        {
            return (IValue)valueReader.GetValue(0);
        }

        throw new Exception("Query не вернул значения хотя ожидался скаляр");
    }
}
