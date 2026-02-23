using System.Data;
using ReData.DemoApp.Database.Entities.Abstract;
using ReData.Query.Core.Types;

namespace ReData.DemoApp.Database.Entities;

/// <summary>
/// Сущность для хранения в БД
/// отображает набор данных
/// </summary>
public sealed record DataSetEntity : BaseEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required Guid DataConnectorId { get; set; }

    public DataConnectorEntity? DataConnector { get; init; }

    public required List<TransformationEntity> Transformations { get; init; }

    public required long? RowsCount { get; set; }

    public required IReadOnlyList<DataSetField>? FieldList { get; set; } // Json
}

public struct DataSetField
{
    public required string Alias { get; init; }

    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
}