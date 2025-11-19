namespace ReData.DemoApplication.Database.Entities.Abstract;


/// <summary>
/// Не большой базовый класс для того что бы не засорять сущности
/// Предназначено для самостоятельных сущностей
/// </summary>
public abstract record BaseEntity : ICreatedAt, IUpdatedAt
{
    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; }
}