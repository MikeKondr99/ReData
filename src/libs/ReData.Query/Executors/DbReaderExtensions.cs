using System.Data.Common;
using System.Runtime.CompilerServices;
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
        List<Dictionary<string, IValue>> result = new();

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

        throw new InvalidOperationException("Scalar query returned no rows.");
    }

    /// <summary>
    /// Streams rows as dictionaries assuming values are already normalized by DomainDbDataReader.
    /// This method does not perform additional type/null normalization.
    /// </summary>
    public static async IAsyncEnumerable<object> ToAsyncEnumerable(
        this DbDataReader reader,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(reader.FieldCount);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }

            yield return row;
        }
    }

    public static async Task<T> GetScalarAsync<T>(this DbDataReader reader, CancellationToken ct = default)
    {
        if (!await reader.ReadAsync(ct))
        {
            throw new InvalidOperationException("Scalar query returned no rows.");
        }

        return reader.GetFieldValue<T>(0);
    }
}
