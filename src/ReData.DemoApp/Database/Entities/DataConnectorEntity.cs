using ReData.DemoApp.Database.Entities.Abstract;
using ReData.Query.Core.Types;

namespace ReData.DemoApp.Database.Entities;

/// <summary>
/// Сущность для хранения в БД
/// Отображает изначальное подключение к данным
/// </summary>
public sealed record DataConnectorEntity : BaseEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string TableName { get; init; }

    public required IReadOnlyList<Field> FieldList { get; set; } // Json
}

public struct Field
{
    public required string Alias { get; init; }

    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
}