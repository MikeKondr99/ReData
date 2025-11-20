namespace ReData.DemoApplication.Database.Entities.Abstract;

/// <summary>
/// Интерфейс для сущности в БД которая будет иметь время создания
/// </summary>
public interface ICreatedAt
{
    public DateTimeOffset CreatedAt { get; init; }
}