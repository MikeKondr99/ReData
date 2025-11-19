namespace ReData.DemoApplication.Database.Entities.Abstract;

/// <summary>
/// Интерфейс для сущности в БД которая будет иметь время создания
/// Рассчитано на работу с помощью <see cref="TimeInterceptor"/>
/// </summary>
public interface ICreatedAt
{
    public DateTimeOffset CreatedAt
    {
        get;
        set;
    }
}