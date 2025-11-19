
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

    public required List<TransformationEntity> Transformations { get; init; }

}

/// <summary>
/// Сущность для хранения в БД
/// Отображает изначальное подключение к данным
/// </summary>
public sealed record DataConnectorEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string TableId { get; init; }
    
    
}