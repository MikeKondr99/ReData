namespace ReData.DemoApp.Database.Entities.Abstract;


/// <summary>
/// Не большой базовый класс для того что бы не засорять сущности
/// Предназначено для самостоятельных сущностей
/// </summary>
public abstract record BaseEntity : ICreatedAt, IUpdatedAt
{
    /// <inheritdoc />
    public required DateTimeOffset CreatedAt { get; init; }

    /// <inheritdoc />
    public required DateTimeOffset UpdatedAt { get; set; }
}