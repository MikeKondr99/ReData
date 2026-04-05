using FluentValidation;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// Запрос на выполнение трансформаций
/// </summary>
public sealed record TransformRequest
{
    /// <summary>
    /// Id коннектора данных выбранного как источник данных
    /// </summary>
    public required Guid DataConnectorId { get; init; }

    /// <summary>
    /// Номер страницы для пагинации
    /// </summary>
    public required uint PageNumber { get; init; }

    /// <summary>
    /// Размер страницы для пагинации
    /// </summary>
    public required uint PageSize { get; init; }

    /// <summary>
    /// Имя по поля по которому нужно отсортировать конечные данные
    /// Если такого поля нет сортировка не происходит
    /// </summary>
    public string? OrderByName { get; init; }

    /// <summary>
    /// Выбрать конечную сортировку по полю <see cref="OrderByName"/> по убыванию
    /// Если такого поля с названием <see cref="OrderByName"/> сортировка не происходит
    /// </summary>
    public bool? OrderByDescending { get; init; }

    /// <summary>
    /// Лист трансформаций выполняемых по очереди
    /// </summary>
    public required List<Transformation> Transformations { get; init; } = new();
}