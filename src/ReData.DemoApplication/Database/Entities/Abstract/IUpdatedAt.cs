using ReData.DemoApplication.Database.Interceptors;

namespace ReData.DemoApplication.Database.Entities.Abstract;

/// <summary>
/// Интерфейс для сущности в БД которая будет иметь время обновления
/// Рассчитано на работу с помощью <see cref="TimeInterceptor"/>
/// </summary>
public interface IUpdatedAt
{
    public DateTimeOffset UpdatedAt
    {
        get;
        set;
    }
}