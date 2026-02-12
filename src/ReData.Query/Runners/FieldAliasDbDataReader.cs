using System.Data.Common;
using ReData.Query.Core.Types;

namespace ReData.Query.Runners;

/// <summary>
/// Декоратор DbDataReader, который подменяет имена колонок на пользовательские alias из доменной модели.
/// Нужен для сценариев, где физические имена колонок технические (например, column1, column2, ...),
/// а наружу должны отдаваться осмысленные alias.
/// </summary>
#pragma warning disable CA1010
public sealed class FieldAliasDbDataReader : DbDataReaderDecorator
{
    private readonly Dictionary<string, int> aliasToOrdinal;
    private readonly string[] aliasNames;

    public FieldAliasDbDataReader(DbDataReader inner, IEnumerable<Field> fields)
        : base(inner)
    {
        int i = 0;
        aliasNames = fields.Select(f => f.Alias).ToArray();
        aliasToOrdinal = aliasNames.ToDictionary(n => n, _ => i++);
    }

    public override string GetName(int ordinal) => aliasNames[ordinal];

    public override int GetOrdinal(string name) => aliasToOrdinal[name];
}
#pragma warning restore CA1010

