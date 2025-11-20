
namespace ReData.DemoApplication.Database.Entities.Abstract;

/// <summary>
/// Интерфейс для сущности в БД которая будет иметь время обновления
/// </summary>
public interface IUpdatedAt
{
    public DateTimeOffset UpdatedAt { get; set; }
}