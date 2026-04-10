using System.Linq.Expressions;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Repositories.Datasets;

namespace ReData.DemoApp.Endpoints.Datasets.GetAll;

/// <summary>
/// Модель отображения набора данных в таблице
/// </summary>
public sealed record DataSetListItem : IProjection<DatasetEntity, DataSetListItem>
{
    /// <summary>
    /// Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Количество строк при последнем сохранении
    /// </summary>
    public required long? RowsCount { get; init; }

    /// <summary>
    /// Конечный набор полей при последнем сохранении
    /// </summary>
    public required IReadOnlyList<DataSetField>? FieldList { get; init; }
    
    public static Expression<Func<DatasetEntity, DataSetListItem>> Projection { get; } = (ds) => new DataSetListItem 
    {
        Id = ds.Id.ToGuid(),
        Name = ds.Name,
        CreatedAt = ds.CreatedAt,
        UpdatedAt = ds.UpdatedAt,
        FieldList = ds.FieldList,
        RowsCount = ds.RowsCount,
    };
    
    public static Func<IQueryable<DatasetEntity>, IQueryable<DatasetEntity>> Include { get; } = q => q;
}
