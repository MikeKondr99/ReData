using System.Data.Common;
using ReData.Query.Core.Types;

namespace ReData.Query.Runners;

/// <summary>
/// DbDataReader, приведенный к доменному контракту ReData:
/// - значения нормализованы к стабильным CLR-типам;
/// - имена колонок соответствуют alias из доменной модели;
/// - доступны доменные типы полей.
/// </summary>
#pragma warning disable CA1010
public sealed class DomainDbDataReader : DbDataReaderDecorator
{
    private readonly Dictionary<string, int> aliasToOrdinal;
    private readonly FieldType[] fieldTypes;

    public DomainDbDataReader(DbDataReader inner, IEnumerable<Field> fields)
        : this(inner, fields.ToArray())
    {
    }

    private DomainDbDataReader(DbDataReader inner, Field[] fields)
        : base(new FieldAliasDbDataReader(new ClrMappedDbDataReader(inner, fields), fields))
    {
        var aliases = fields.Select(f => f.Alias).ToArray();
        fieldTypes = fields.Select(f => f.Type).ToArray();
        aliasToOrdinal = aliases
            .Select((alias, index) => (alias, index))
            .ToDictionary(x => x.alias, x => x.index);
    }

    public FieldType GetDomainType(int ordinal)
    {
        if ((uint)ordinal >= (uint)fieldTypes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Доменный тип поля не найден.");
        }

        return fieldTypes[ordinal];
    }

    public FieldType GetDomainType(string name)
    {
        if (!aliasToOrdinal.TryGetValue(name, out var ordinal))
        {
            throw new IndexOutOfRangeException($"Поле '{name}' не найдено.");
        }

        return fieldTypes[ordinal];
    }

    public IReadOnlyList<FieldType> DomainFieldTypes => fieldTypes;
}

#pragma warning restore CA1010
