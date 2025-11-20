using ReData.DemoApplication.Database.Entities.Abstract;

namespace ReData.DemoApplication.Database.Entities;

/// <summary>
/// Сущность для хранения в БД
/// Отображает набор данных
/// </summary>
public sealed record DataSetEntity : BaseEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required Guid DataConnectorId { get; init; }

    public DataConnectorEntity DataConnector { get; init; }
    public required List<TransformationEntity> Transformations { get; init; }
}